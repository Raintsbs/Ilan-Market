"use client";

import { useRef, useState } from "react";
import { getMediaUrl } from "@/lib/image";

type PanoramaViewerProps = {
  path: string;
  title?: string;
};

export function PanoramaViewer({ path, title = "360° tur" }: PanoramaViewerProps) {
  const url = getMediaUrl(path);
  const containerRef = useRef<HTMLDivElement>(null);
  const [offset, setOffset] = useState(0);
  const dragging = useRef(false);
  const startX = useRef(0);
  const startOffset = useRef(0);

  if (!url) return null;

  const onPointerDown = (e: React.PointerEvent) => {
    dragging.current = true;
    startX.current = e.clientX;
    startOffset.current = offset;
    (e.target as HTMLElement).setPointerCapture(e.pointerId);
  };

  const onPointerMove = (e: React.PointerEvent) => {
    if (!dragging.current) return;
    const delta = e.clientX - startX.current;
    setOffset(startOffset.current + delta * 0.4);
  };

  const onPointerUp = () => {
    dragging.current = false;
  };

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-black dark:border-slate-700">
      <div className="border-b border-slate-800 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-400">
        {title} — sürükleyerek gezinin
      </div>
      <div
        ref={containerRef}
        className="relative h-64 cursor-grab overflow-hidden active:cursor-grabbing sm:h-80"
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerUp}
        onPointerLeave={onPointerUp}
      >
        <div
          className="absolute inset-y-0 h-full w-[200%] max-w-none bg-cover bg-center"
          style={{
            backgroundImage: `url(${url})`,
            transform: `translateX(${offset}px)`,
            transition: dragging.current ? "none" : "transform 0.1s ease-out",
          }}
        />
      </div>
    </div>
  );
}
