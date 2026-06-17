import type {
  AdminCategory,
  AdminDashboard,
  AdminUser,
  AdminUserDetail,
  Advertisement,
  AdvertisementFilter,
  ApiResponse,
  AuditLog,
  BlogPost,
  CityLocation,
  MarketplaceOrder,
  PagedResult,
  ReportItem,
  StaticPage,
  AdminAdPackage,
  AdminReviewItem,
  VerificationRequest,
  Coupon,
  BulkImportResult,
} from "./types";
import { adminRequest } from "./adminAuth";
import { notifyAdsChanged } from "./adsSync";

async function mutateAds<T>(call: () => Promise<ApiResponse<T>>) {
  const res = await call();
  if (res.success) notifyAdsChanged();
  return res;
}

export const adminApi = {
  dashboard: () => adminRequest<AdminDashboard>("/api/admin/dashboard"),

  getAds: (filter: AdvertisementFilter & { status?: number; expiredOnly?: boolean; archivedOnly?: boolean }) => {
    const p = new URLSearchParams();
    p.set("adminMode", "true");
    if (filter.search) p.set("search", filter.search);
    if (filter.categoryId) p.set("categoryId", String(filter.categoryId));
    if (filter.status !== undefined) p.set("status", String(filter.status));
    if (filter.page) p.set("page", String(filter.page));
    p.set("pageSize", String(filter.pageSize ?? 20));
    if (filter.expiredOnly) p.set("expiredOnly", "true");
    if (filter.archivedOnly) p.set("archivedOnly", "true");
    return adminRequest<PagedResult<Advertisement>>(`/api/admin/advertisements?${p}`);
  },

  approveAd: (id: number) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}/approve`, { method: "POST" }),
    ),
  rejectAd: (id: number, reason?: string) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}/reject`, {
        method: "POST",
        body: JSON.stringify({ reason }),
      }),
    ),
  deleteAd: (id: number) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}`, { method: "DELETE" }),
    ),
  extendAd: (id: number, extendDays: number) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}/extend`, {
        method: "POST",
        body: JSON.stringify({ extendDays }),
      }),
    ),
  archiveAd: (id: number) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}/archive`, { method: "POST" }),
    ),
  featureAd: (id: number, featuredDays: number) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/advertisements/${id}/featured`, {
        method: "POST",
        body: JSON.stringify({ isFeatured: true, featuredDays }),
      }),
    ),
  bulkAds: (ids: number[], action: string, reason?: string) =>
    mutateAds(() =>
      adminRequest<number>("/api/admin/advertisements/bulk", {
        method: "POST",
        body: JSON.stringify({ ids, action, reason }),
      }),
    ),
  adHistory: (id: number) =>
    adminRequest<AuditLog[]>(`/api/admin/advertisements/${id}/history`),

  getReports: (status = "open") =>
    adminRequest<ReportItem[]>(`/api/admin/reports?status=${status}`),
  resolveReport: (id: number, action: string, note?: string, rejectAd?: boolean) =>
    mutateAds(() =>
      adminRequest<void>(`/api/admin/reports/${id}/resolve`, {
        method: "POST",
        body: JSON.stringify({ action, note, rejectAd }),
      }),
    ),

  getUsers: (search?: string, role?: string) => {
    const p = new URLSearchParams();
    if (search) p.set("search", search);
    if (role) p.set("role", role);
    const qs = p.toString();
    return adminRequest<AdminUser[]>(`/api/admin/users${qs ? `?${qs}` : ""}`);
  },
  getUser: (id: number) => adminRequest<AdminUserDetail>(`/api/admin/users/${id}`),
  moderateUser: (id: number, body: Record<string, unknown>) =>
    adminRequest<void>(`/api/admin/users/${id}/moderate`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  getCategories: () => adminRequest<AdminCategory[]>("/api/admin/categories"),
  saveCategory: (dto: AdminCategory, id?: number) =>
    adminRequest<void>(id ? `/api/admin/categories/${id}` : "/api/admin/categories", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(dto),
    }),
  deleteCategory: (id: number) =>
    adminRequest<void>(`/api/admin/categories/${id}`, { method: "DELETE" }),

  getPages: () => adminRequest<StaticPage[]>("/api/admin/pages"),
  savePage: (dto: StaticPage) =>
    adminRequest<void>("/api/admin/pages", { method: "PUT", body: JSON.stringify(dto) }),

  getCities: () => adminRequest<CityLocation[]>("/api/admin/cities"),
  saveCity: (dto: CityLocation, id?: number) =>
    adminRequest<void>(id ? `/api/admin/cities/${id}` : "/api/admin/cities", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(dto),
    }),
  deleteCity: (id: number) =>
    adminRequest<void>(`/api/admin/cities/${id}`, { method: "DELETE" }),

  getBlog: () => adminRequest<BlogPost[]>("/api/admin/blog"),
  saveBlog: (dto: BlogPost, id?: number) =>
    adminRequest<void>(id ? `/api/admin/blog/${id}` : "/api/admin/blog", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(dto),
    }),
  deleteBlog: (id: number) =>
    adminRequest<void>(`/api/admin/blog/${id}`, { method: "DELETE" }),

  getDisputedOrders: () =>
    adminRequest<MarketplaceOrder[]>("/api/admin/marketplace-orders/disputed"),

  getMarketplaceOrders: (filter: {
    status?: number;
    search?: string;
    page?: number;
    pageSize?: number;
  }) => {
    const p = new URLSearchParams();
    if (filter.status !== undefined) p.set("status", String(filter.status));
    if (filter.search) p.set("search", filter.search);
    if (filter.page) p.set("page", String(filter.page));
    p.set("pageSize", String(filter.pageSize ?? 20));
    return adminRequest<PagedResult<MarketplaceOrder>>(`/api/admin/marketplace-orders?${p}`);
  },

  cancelOrder: (orderId: number) =>
    adminRequest<MarketplaceOrder>(`/api/admin/marketplace-orders/${orderId}/cancel`, {
      method: "POST",
    }),

  markRefund: (orderId: number, note?: string) =>
    adminRequest<MarketplaceOrder>(`/api/admin/marketplace-orders/${orderId}/refund`, {
      method: "POST",
      body: JSON.stringify({ note }),
    }),

  markSellerPayout: (orderId: number, note?: string) =>
    adminRequest<MarketplaceOrder>(`/api/admin/marketplace-orders/${orderId}/seller-payout`, {
      method: "POST",
      body: JSON.stringify({ note }),
    }),

  resolveDispute: (orderId: number, resolution: "complete" | "cancel", adminNote?: string) =>
    adminRequest<MarketplaceOrder>(
      `/api/admin/marketplace-orders/${orderId}/resolve-dispute`,
      {
        method: "POST",
        body: JSON.stringify({ resolution, adminNote }),
      },
    ),

  getAdPackages: () => adminRequest<AdminAdPackage[]>("/api/admin/ad-packages"),
  saveAdPackage: (dto: AdminAdPackage, id?: number) =>
    adminRequest<void>(id ? `/api/admin/ad-packages/${id}` : "/api/admin/ad-packages", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(dto),
    }),
  deleteAdPackage: (id: number) =>
    adminRequest<void>(`/api/admin/ad-packages/${id}`, { method: "DELETE" }),

  getReviews: (type?: string, take = 50) => {
    const p = new URLSearchParams();
    if (type) p.set("type", type);
    p.set("take", String(take));
    return adminRequest<AdminReviewItem[]>(`/api/admin/reviews?${p}`);
  },

  hideReview: (reviewType: string, id: number, hidden: boolean) =>
    adminRequest<void>(`/api/admin/reviews/${reviewType}/${id}/hide?hidden=${hidden}`, {
      method: "POST",
    }),

  deleteReview: (reviewType: string, id: number) =>
    adminRequest<void>(`/api/admin/reviews/${reviewType}/${id}`, { method: "DELETE" }),

  getPendingVerifications: () =>
    adminRequest<VerificationRequest[]>("/api/admin/verifications/pending"),

  reviewVerification: (id: number, approve: boolean, adminNote?: string) =>
    adminRequest<void>(`/api/admin/verifications/${id}/review`, {
      method: "POST",
      body: JSON.stringify({ approve, adminNote }),
    }),

  importAds: (csv: string) =>
    adminRequest<BulkImportResult>("/api/admin/advertisements/import", {
      method: "POST",
      body: JSON.stringify(csv),
    }),

  getCoupons: () => adminRequest<Coupon[]>("/api/admin/coupons"),

  saveCoupon: (dto: Coupon) =>
    adminRequest<void>(dto.id ? `/api/admin/coupons/${dto.id}` : "/api/admin/coupons", {
      method: dto.id ? "PUT" : "POST",
      body: JSON.stringify(dto),
    }),
};

export function isStaff(roles?: string[]) {
  return roles?.some((r) => r === "Admin" || r === "Moderator") ?? false;
}

export function isAdmin(roles?: string[]) {
  return roles?.includes("Admin") ?? false;
}
