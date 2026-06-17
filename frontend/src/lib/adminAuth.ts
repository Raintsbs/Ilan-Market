import type { ApiResponse, AuthResponse, LoginRequest, UserProfile } from "./types";
import { ApiError } from "./api";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";

const TOKEN_KEY = "admin_session_token";
const USER_KEY = "admin_session_user";

export function getAdminToken(): string | null {
  if (typeof window === "undefined") return null;
  return sessionStorage.getItem(TOKEN_KEY);
}

export function loadAdminUser(): UserProfile | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = sessionStorage.getItem(USER_KEY);
    if (!raw) return null;
    const cached = JSON.parse(raw) as Record<string, unknown>;
    return {
      userId: Number(cached.userId),
      email: String(cached.email ?? ""),
      userName: String(cached.userName ?? cached.email ?? ""),
      firstName: String(cached.firstName ?? ""),
      lastName: String(cached.lastName ?? ""),
      profileImagePath: cached.profileImagePath
        ? String(cached.profileImagePath)
        : undefined,
      roles: Array.isArray(cached.roles) ? (cached.roles as string[]) : [],
    };
  } catch {
    return null;
  }
}

export function persistAdminSession(auth: AuthResponse) {
  sessionStorage.setItem(TOKEN_KEY, auth.token);
  sessionStorage.setItem(
    USER_KEY,
    JSON.stringify({
      userId: auth.userId,
      email: auth.email,
      userName: auth.email,
      roles: auth.roles,
      firstName: "",
      lastName: "",
    }),
  );
}

export function clearAdminSession() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
}

async function parseResponse<T>(res: Response): Promise<ApiResponse<T>> {
  const text = await res.text();
  let body: ApiResponse<T>;

  if (!text.trim()) {
    if (!res.ok) throw new ApiError(res.statusText || "İstek başarısız", res.status);
    body = { success: true, message: "", data: undefined };
  } else {
    try {
      body = JSON.parse(text) as ApiResponse<T>;
    } catch {
      throw new ApiError(
        text.length > 200 ? `${text.slice(0, 200)}…` : text || res.statusText,
        res.status,
      );
    }
  }

  if (!res.ok) {
    throw new ApiError(body.message || res.statusText, res.status);
  }
  return body;
}

export async function adminPanelLogin(data: LoginRequest): Promise<AuthResponse> {
  const res = await fetch(`${API_URL}/api/admin/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  const text = await res.text();
  let body: ApiResponse<AuthResponse>;
  try {
    body = text.trim()
      ? (JSON.parse(text) as ApiResponse<AuthResponse>)
      : { success: false, message: res.statusText };
  } catch {
    throw new ApiError(res.statusText || "Giriş başarısız", res.status);
  }

  if (!res.ok) {
    throw new ApiError(
      body.message ||
        (res.status === 403
          ? "Bu hesabın yönetim paneline erişimi yok."
          : "Giriş başarısız"),
      res.status,
    );
  }

  if (!body.success || !body.data) {
    throw new ApiError(body.message || "Giriş başarısız", 401);
  }
  return body.data;
}

export async function adminRequest<T>(
  path: string,
  options: RequestInit = {},
): Promise<ApiResponse<T>> {
  const headers = new Headers(options.headers);

  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  const token = getAdminToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(`${API_URL}${path}`, { ...options, headers });
  return parseResponse<T>(res);
}
