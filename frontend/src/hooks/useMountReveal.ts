"use client";

import { useEffect, useState } from "react";

export function useMountReveal() {
  const [active, setActive] = useState(false);

  useEffect(() => {
    setActive(false);
    let raf1 = 0;
    let raf2 = 0;
    raf1 = requestAnimationFrame(() => {
      raf2 = requestAnimationFrame(() => setActive(true));
    });
    return () => {
      cancelAnimationFrame(raf1);
      cancelAnimationFrame(raf2);
    };
  }, []);

  return active;
}
