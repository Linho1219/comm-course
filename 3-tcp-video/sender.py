import argparse
import ctypes
import socket
import tkinter as tk
from tkinter import messagebox, ttk

import cv2
from PIL import Image, ImageTk

from common import (
    DEFAULT_HOST,
    DEFAULT_JPEG_QUALITY,
    DEFAULT_PORT,
    encode_frame,
    open_camera,
    probe_camera_indices,
    send_packet,
)


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
    parser = argparse.ArgumentParser(description="TCP 视频发送端")
    parser.add_argument("--host", default=DEFAULT_HOST, help="接收端地址")
    parser.add_argument("--port", type=int, default=DEFAULT_PORT, help="接收端端口")
    parser.add_argument("--quality", type=int, default=DEFAULT_JPEG_QUALITY, help="JPEG 压缩质量 0-100")
    return parser.parse_args()


class SenderApp:
    def __init__(self, root: tk.Tk, host: str, port: int, quality: int) -> None:
        self.root = root
        self.host = host
        self.port = port
        self.quality = quality

        self.root.title("TCP 视频发送端")
        self.root.protocol("WM_DELETE_WINDOW", self.on_close)

        self.status_var = tk.StringVar(value="正在检测摄像头...")
        self.camera_var = tk.StringVar()

        self.cameras = probe_camera_indices()
        self.capture = None
        self.client = None
        self.preview_image = None
        self.running = False

        self._build_ui()
        self._init_camera()
        self._connect_receiver()
        self.running = True
        self._update_frame()

    def _build_ui(self) -> None:
        main_frame = ttk.Frame(self.root, padding=12)
        main_frame.pack(fill="both", expand=True)

        control_frame = ttk.Frame(main_frame)
        control_frame.pack(fill="x")

        ttk.Label(control_frame, text="摄像头:").pack(side="left")
        self.camera_combo = ttk.Combobox(
            control_frame,
            textvariable=self.camera_var,
            state="readonly",
            width=18,
        )
        self.camera_combo.pack(side="left", padx=(8, 12))
        self.camera_combo.bind("<<ComboboxSelected>>", self.on_camera_selected)

        ttk.Button(control_frame, text="刷新设备", command=self.refresh_cameras).pack(side="left")
        ttk.Label(control_frame, text=f"发送到 {self.host}:{self.port}").pack(side="right")

        self.preview_label = ttk.Label(main_frame)
        self.preview_label.pack(fill="both", expand=True, pady=12)
        self.preview_label.configure(anchor="center")

        status_bar = ttk.Label(main_frame, textvariable=self.status_var, anchor="w")
        status_bar.pack(fill="x")

    def _init_camera(self) -> None:
        if not self.cameras:
            self.status_var.set("没有检测到可用摄像头")
            messagebox.showerror("发送端", "没有检测到可用摄像头")
            return

        values = [str(index) for index in self.cameras]
        self.camera_combo["values"] = values
        self.camera_var.set(values[0])
        self._switch_camera(int(values[0]))

    def _connect_receiver(self) -> None:
        try:
            self.client = socket.create_connection((self.host, self.port), timeout=5)
            self.client.settimeout(None)
            self.status_var.set(f"已连接到接收端 {self.host}:{self.port}")
        except OSError as exc:
            self.client = None
            self.status_var.set(f"连接失败: {exc}")
            messagebox.showerror("发送端", f"无法连接接收端 {self.host}:{self.port}\n{exc}")

    def refresh_cameras(self) -> None:
        self.cameras = probe_camera_indices()
        values = [str(index) for index in self.cameras]
        self.camera_combo["values"] = values
        if not values:
            self.camera_var.set("")
            self._release_camera()
            self.status_var.set("没有检测到可用摄像头")
            return

        current_value = self.camera_var.get()
        selected = current_value if current_value in values else values[0]
        self.camera_var.set(selected)
        self._switch_camera(int(selected))

    def on_camera_selected(self, _event=None) -> None:
        selected = self.camera_var.get()
        if selected:
            self._switch_camera(int(selected))

    def _switch_camera(self, camera_index: int) -> None:
        self._release_camera()
        try:
            self.capture = open_camera(camera_index)
            self.status_var.set(f"当前摄像头: {camera_index} | 已连接 {self.host}:{self.port}")
        except RuntimeError as exc:
            self.capture = None
            self.status_var.set(str(exc))
            messagebox.showerror("发送端", str(exc))

    def _release_camera(self) -> None:
        if self.capture is not None:
            self.capture.release()
            self.capture = None

    def _update_frame(self) -> None:
        if not self.running:
            return

        if self.capture is not None:
            ok, frame = self.capture.read()
            if ok:
                self._show_frame(frame)
                if self.client is not None:
                    try:
                        payload = encode_frame(frame, self.quality)
                        send_packet(self.client, payload)
                    except OSError as exc:
                        self.status_var.set(f"发送失败: {exc}")
                        self._close_client()
            else:
                self.status_var.set("读取摄像头画面失败")

        self.root.after(30, self._update_frame)

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

    def _close_client(self) -> None:
        if self.client is not None:
            self.client.close()
            self.client = None

    def on_close(self) -> None:
        self.running = False
        self._release_camera()
        self._close_client()
        self.root.destroy()


def main() -> None:
    args = parse_args()
    enable_high_dpi()
    root = tk.Tk()
    SenderApp(root, args.host, args.port, args.quality)
    root.mainloop()


if __name__ == "__main__":
    main()
