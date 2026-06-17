/** Site oturumu — localStorage + diğer bileşenlere bildirim */

export function clearAuthSession(blockMessage?: string) {
  if (typeof window === "undefined") return;
  localStorage.removeItem("auth_token");
  localStorage.removeItem("auth_user");
  if (blockMessage) {
    sessionStorage.setItem("auth_block_message", blockMessage);
  }
  window.dispatchEvent(new CustomEvent("auth:session-ended"));
}
