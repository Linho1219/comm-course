import socket
import struct
from typing import Iterator, Optional

import cv2
import numpy as np


HEADER_SIZE = 4
DEFAULT_HOST = "127.0.0.1"
DEFAULT_PORT = 8888
DEFAULT_JPEG_QUALITY = 80


def probe_camera_indices(max_index: int = 5) -> list[int]:
    available: list[int] = []
    for index in range(max_index + 1):
        capture = cv2.VideoCapture(index)
        try:
            if capture.isOpened():
                ok, _ = capture.read()
                if ok:
                    available.append(index)
        finally:
            capture.release()
    return available


def open_camera(camera_index: int) -> cv2.VideoCapture:
    capture = cv2.VideoCapture(camera_index)
    if not capture.isOpened():
        capture.release()
        raise RuntimeError(f"无法打开摄像头 {camera_index}")
    return capture


def encode_frame(frame: np.ndarray, jpeg_quality: int = DEFAULT_JPEG_QUALITY) -> bytes:
    ok, encoded = cv2.imencode(
        ".jpg",
        frame,
        [int(cv2.IMWRITE_JPEG_QUALITY), int(jpeg_quality)],
    )
    if not ok:
        raise RuntimeError("视频帧编码失败")
    return encoded.tobytes()


def decode_frame(frame_bytes: bytes) -> np.ndarray:
    buffer = np.frombuffer(frame_bytes, dtype=np.uint8)
    frame = cv2.imdecode(buffer, cv2.IMREAD_COLOR)
    if frame is None:
        raise RuntimeError("视频帧解码失败")
    return frame


def send_packet(sock: socket.socket, payload: bytes) -> None:
    header = struct.pack("!I", len(payload))
    sock.sendall(header + payload)


def recv_exact(sock: socket.socket, size: int) -> Optional[bytes]:
    chunks = bytearray()
    while len(chunks) < size:
        chunk = sock.recv(size - len(chunks))
        if not chunk:
            return None
        chunks.extend(chunk)
    return bytes(chunks)


def receive_packets(sock: socket.socket) -> Iterator[bytes]:
    while True:
        header = recv_exact(sock, HEADER_SIZE)
        if header is None:
            break
        (payload_size,) = struct.unpack("!I", header)
        payload = recv_exact(sock, payload_size)
        if payload is None:
            break
        yield payload
