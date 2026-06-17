"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { api, ApiError } from "@/lib/api";
import { clearAuthSession } from "@/lib/authSession";
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from "@/lib/types";

interface AuthContextValue {
  user: UserProfile | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  loginExternal: (provider: string, idToken: string, email?: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function loadCachedUser(): UserProfile | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem("auth_user");
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

function persistAuth(auth: AuthResponse, profile?: Partial<UserProfile>) {
  localStorage.setItem("auth_token", auth.token);
  if (auth.refreshToken) {
    localStorage.setItem("auth_refresh_token", auth.refreshToken);
  }
  localStorage.setItem(
    "auth_user",
    JSON.stringify({
      userId: auth.userId,
      email: auth.email,
      userName: profile?.userName ?? profile?.email ?? auth.email,
      roles: auth.roles,
      firstName: profile?.firstName ?? "",
      lastName: profile?.lastName ?? "",
      profileImagePath: profile?.profileImagePath ?? "",
    }),
  );
}

function userFromAuthResponse(auth: AuthResponse): UserProfile {
  const cached = loadCachedUser();
  return {
    userId: auth.userId,
    email: auth.email,
    userName: cached?.userId === auth.userId ? cached.userName : auth.email,
    firstName: cached?.userId === auth.userId ? cached.firstName : "",
    lastName: cached?.userId === auth.userId ? cached.lastName : "",
    profileImagePath:
      cached?.userId === auth.userId ? cached.profileImagePath : undefined,
    roles: auth.roles,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshProfile = useCallback(async () => {
    const stored = localStorage.getItem("auth_token");
    if (!stored) {
      setUser(null);
      setToken(null);
      return;
    }

    setToken(stored);
    const cached = loadCachedUser();
    if (cached) setUser(cached);

    try {
      const res = await api.me();
      if (res.success && res.data) {
        setUser(res.data);
        persistAuth(
          {
            token: stored,
            userId: res.data.userId,
            email: res.data.email,
            roles: res.data.roles,
          },
          res.data,
        );
      }
    } catch (err) {
      if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
        clearAuthSession(err.status === 403 ? err.message : undefined);
        setUser(null);
        setToken(null);
      }
    }
  }, []);

  useEffect(() => {
    const onSessionEnded = () => {
      setUser(null);
      setToken(null);
    };
    window.addEventListener("auth:session-ended", onSessionEnded);
    return () => window.removeEventListener("auth:session-ended", onSessionEnded);
  }, []);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      const stored = localStorage.getItem("auth_token");
      if (stored) {
        setToken(stored);
        setUser(loadCachedUser());
        await refreshProfile();
      }
      if (!cancelled) setIsLoading(false);
    })();

    return () => {
      cancelled = true;
    };
  }, [refreshProfile]);

  const applyAuthResponse = useCallback(async (auth: AuthResponse) => {
    persistAuth(auth);
    setToken(auth.token);
    setUser(userFromAuthResponse(auth));

    try {
      const res = await api.me();
      if (res.success && res.data) {
        setUser(res.data);
        persistAuth(auth, res.data);
      }
    } catch {
      // Giriş başarılı; profil sonra yenilenebilir
    }
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const res = await api.login(data);
    if (!res.success || !res.data) throw new ApiError(res.message, 401);
    await applyAuthResponse(res.data);
  }, [applyAuthResponse]);

  const loginExternal = useCallback(
    async (provider: string, idToken: string, email?: string) => {
      const res = await api.externalLogin(provider, idToken, email);
      if (!res.success || !res.data) throw new ApiError(res.message, 401);
      await applyAuthResponse(res.data);
    },
    [applyAuthResponse],
  );

  const register = useCallback(
    async (data: RegisterRequest) => {
      const res = await api.register(data);
      if (!res.success || !res.data) throw new ApiError(res.message, 400);
      persistAuth(res.data, {
        firstName: data.firstName,
        lastName: data.lastName,
      });
      setToken(res.data.token);
      setUser({
        userId: res.data.userId,
        email: res.data.email,
        userName: res.data.email,
        firstName: data.firstName,
        lastName: data.lastName,
        roles: res.data.roles,
      });
      try {
        const meRes = await api.me();
        if (meRes.success && meRes.data) {
          setUser(meRes.data);
          persistAuth(res.data, meRes.data);
        }
      } catch {
        /* keep optimistic profile */
      }
    },
    [],
  );

  const logout = useCallback(() => {
    const refresh = localStorage.getItem("auth_refresh_token");
    if (refresh) {
      void api.logout(refresh).catch(() => undefined);
    }
    localStorage.removeItem("auth_token");
    localStorage.removeItem("auth_refresh_token");
    localStorage.removeItem("auth_user");
    setUser(null);
    setToken(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      token,
      isLoading,
      isAuthenticated: !!token && !!user,
      login,
      loginExternal,
      register,
      logout,
      refreshProfile,
    }),
    [user, token, isLoading, login, loginExternal, register, logout, refreshProfile],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
