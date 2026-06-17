export type RealtimeChannel = "notification" | "messages" | "offers";

export function dispatchRealtime(channel: RealtimeChannel, payload?: unknown) {
  if (typeof window === "undefined") return;
  window.dispatchEvent(
    new CustomEvent(`realtime:${channel}`, { detail: payload }),
  );
}

export function subscribeRealtime(
  channel: RealtimeChannel,
  handler: (payload: unknown) => void,
) {
  if (typeof window === "undefined") return () => {};
  const eventName = `realtime:${channel}`;
  const listener = (e: Event) => handler((e as CustomEvent).detail);
  window.addEventListener(eventName, listener);
  return () => window.removeEventListener(eventName, listener);
}
