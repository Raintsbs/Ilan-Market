"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { ApiError } from "@/lib/api";
import {
  adminPanelLogin,
  clearAdminSession,
  getAdminToken,
  loadAdminUser,
  persistAdminSession,
} from "@/lib/adminAuth";
import { isStaff } from "@/lib/admin";
import type { LoginRequest, UserProfile } from "@/lib/types";

interface AdminAuthContextValue {
  user: UserProfile | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AdminAuthContext = createContext<AdminAuthContextValue | null>(null);

export function AdminAuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = getAdminToken();
    if (token) {
      const cached = loadAdminUser();
      if (cached && isStaff(cached.roles)) {
        setUser(cached);
      } else {
        clearAdminSession();
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const auth = await adminPanelLogin(data);
    if (!isStaff(auth.roles)) {
      throw new ApiError("Bu hesabın yönetim paneline erişimi yok.", 403);
    }
    persistAdminSession(auth);
    setUser({
      userId: auth.userId,
      email: auth.email,
      userName: auth.email,
      firstName: "",
      lastName: "",
      roles: auth.roles,
    });
  }, []);

  const logout = useCallback(() => {
    clearAdminSession();
    setUser(null);
  }, []);

  const isAuthenticated = !!user && !!getAdminToken();

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated,
      login,
      logout,
    }),
    [user, isLoading, isAuthenticated, login, logout],
  );

  return (
    <AdminAuthContext.Provider value={value}>{children}</AdminAuthContext.Provider>
  );
}

export function useAdminAuth() {
  const ctx = useContext(AdminAuthContext);
  if (!ctx) throw new Error("useAdminAuth must be used within AdminAuthProvider");
  return ctx;
}
