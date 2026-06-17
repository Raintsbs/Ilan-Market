import { API_URL, isApiReachable } from "./api";

let connection: import("@microsoft/signalr").HubConnection | null = null;
let starting: Promise<boolean> | null = null;

function getStoredToken(): string {
  if (typeof window === "undefined") return "";
  return localStorage.getItem("auth_token")?.trim() ?? "";
}

/** SignalR bağlantısı — başarısız olursa false döner (polling yedek kalır). */
export async function startRealtimeConnection(
  onEvent: (channel: string, payload: unknown) => void,
): Promise<boolean> {
  if (typeof window === "undefined") return false;

  const token = getStoredToken();
  if (!token) return false;

  if (!(await isApiReachable())) return false;

  try {
    const signalR = await import("@microsoft/signalr");

    if (connection?.state === signalR.HubConnectionState.Connected) return true;

    if (connection) {
      await connection.stop().catch(() => {});
      connection = null;
    }

    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/app`, {
        accessTokenFactory: () => getStoredToken(),
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.None)
      .build();

    connection.on("event", (channel: string, payload: unknown) => {
      onEvent(channel, payload);
    });

    starting = connection
      .start()
      .then(() => true)
      .catch(() => {
        connection = null;
        return false;
      })
      .finally(() => {
        starting = null;
      });

    return (await starting) ?? false;
  } catch {
    connection = null;
    starting = null;
    return false;
  }
}

export async function stopRealtimeConnection() {
  if (connection) {
    await connection.stop().catch(() => {});
    connection = null;
  }
  starting = null;
}
