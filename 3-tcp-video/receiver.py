import argparse
import ctypes
import socket
import threading
import tkinter as tk
from datetime import datetime
from pathlib import Path
from tkinter import messagebox, ttk

import cv2
from PIL import Image, ImageTk

from common import DEFAULT_HOST, DEFAULT_PORT, decode_frame, receive_packets


PREVIEW_WIDTH = 640
PREVIEW_HEIGHT = 480


def enable_high_dpi() -> None:
    try:
        ctypes.windll.shcore.SetProcessDpiAwareness(1)
    except Exception:
        try:
            ctypes.windll.user32.SetProcessDPIAware()
        except Exception:
            pass


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="TCP 视频接收端")
    parser.add_argument("--host", default=DEFAULT_HOST, help="监听地址")
    parser.add_argument("--port", type=int, default=DEFAULT_PORT, help="监听端口")
    parser.add_argument("--output-dir", default="captures", help="单帧图片保存目录")
    return parser.parse_args()


def save_frame(frame, output_dir: Path) -> Path:
    output_dir.mkdir(parents=True, exist_ok=True)
    file_name = datetime.now().strftime("frame_%Y%m%d_%H%M%S_%f.jpg")
    output_path = output_dir / file_name
    cv2.imwrite(str(output_path), frame)
    return output_path


class ReceiverApp:
    def __init__(self, root: tk.Tk, host: str, port: int, output_dir: Path) -> None:
        self.root = root
        self.host = host
        self.port = port
        self.output_dir = output_dir

        self.root.title("TCP 视频接收端")
        self.root.protocol("WM_DELETE_WINDOW", self.on_close)

        self.status_var = tk.StringVar(value=f"监听中: {host}:{port}")
        self.preview_image = None
        self.latest_frame = None
        self.running = True

        self.server_socket = None
        self.client_socket = None
        self.receiver_thread = None

        self._build_ui()
        self._start_server()

    def _build_ui(self) -> None:
        main_frame = ttk.Frame(self.root, padding=12)
        main_frame.pack(fill="both", expand=True)

        top_frame = ttk.Frame(main_frame)
        top_frame.pack(fill="x")

        ttk.Label(top_frame, text=f"监听地址 {self.host}:{self.port}").pack(side="left")
        ttk.Button(top_frame, text="保存当前帧", command=self.save_current_frame).pack(side="right")

        self.preview_label = ttk.Label(main_frame)
        self.preview_label.pack(fill="both", expand=True, pady=12)
        self.preview_label.configure(anchor="center")

        ttk.Label(main_frame, textvariable=self.status_var, anchor="w").pack(fill="x")

    def _start_server(self) -> None:
        self.receiver_thread = threading.Thread(target=self._server_loop, daemon=True)
        self.receiver_thread.start()

    def _server_loop(self) -> None:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server:
                self.server_socket = server
                server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
                server.bind((self.host, self.port))
                server.listen(1)
                self._set_status(f"监听中: {self.host}:{self.port}，等待发送端连接")

                conn, addr = server.accept()
                self.client_socket = conn
                self._set_status(f"已连接发送端: {addr[0]}:{addr[1]}")

                with conn:
                    for payload in receive_packets(conn):
                        if not self.running:
                            break
                        frame = decode_frame(payload)
                        self.latest_frame = frame
                        self.root.after(0, self._show_frame, frame)

                self._set_status("发送端已断开连接")
        except OSError as exc:
            if self.running:
                self._set_status(f"接收异常: {exc}")
                self.root.after(0, lambda: messagebox.showerror("接收端", str(exc)))

    def _show_frame(self, frame) -> None:
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        image = Image.fromarray(rgb_frame)
        image = self._fit_image(image)
        self.preview_image = ImageTk.PhotoImage(image=image)
        self.preview_label.configure(image=self.preview_image)

    def _fit_image(self, image: Image.Image) -> Image.Image:
        label_width = max(self.preview_label.winfo_width(), PREVIEW_WIDTH)
        label_height = max(self.preview_label.winfo_height(), PREVIEW_HEIGHT)
        fitted = image.copy()
        fitted.thumbnail((label_width, label_height), Image.Resampling.LANCZOS)
        return fitted

    def _set_status(self, message: str) -> None:
        self.root.after(0, self.status_var.set, message)

    def save_current_frame(self) -> None:
        if self.latest_frame is None:
            messagebox.showinfo("接收端", "当前还没有收到视频帧")
            return

        output_path = save_frame(self.latest_frame, self.output_dir)
        self.status_var.set(f"已保存单帧: {output_path}")
        messagebox.showinfo("接收端", f"已保存到:\n{output_path}")

    def on_close(self) -> None:
        self.running = False
        if self.client_socket is not None:
            self.client_socket.close()
        if self.server_socket is not None:
            self.server_socket.close()
        self.root.destroy()


def main() -> None:
    args = parse_args()
    enable_high_dpi()
    root = tk.Tk()
    ReceiverApp(root, args.host, args.port, Path(args.output_dir))
    root.mainloop()


if __name__ == "__main__":
    main()
