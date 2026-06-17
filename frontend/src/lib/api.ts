import type {
  AdAnalytics,
  AdPackage,
  PackageExperiment,
  Advertisement,
  PublicBlogPost,
  PublicStaticPage,
  PublicStaticPageListItem,
  AdvertisementCreateInput,
  AdvertisementFilter,
  AdvertisementUpdateInput,
  AnalyticsOverview,
  ApiResponse,
  AppNotification,
  AuthResponse,
  CargoCarrier,
  Category,
  CategoryTreeNode,
  District,
  MarketplaceOrder,
  Neighborhood,
  Province,
  SellerRatingSummary,
  SellerReview,
  AdvertisementReview,
  AdvertisementRatingSummary,
  ChatMessage,
  CheckoutResult,
  LoginRequest,
  MapListing,
  MessageThread,
  MyAdCounts,
  Offer,
  PagedResult,
  PhoneReveal,
  RegisterRequest,
  PriceHistoryPoint,
  SellerAnalytics,
  SellerProfile,
  UserProfile,
  AuctionInfo,
  TramerResult,
  SavedSearch,
  PayMarketplaceOrderResult,
  VerificationRequest,
  ListingQuestion,
  SellerFollow,
  UpdateStoreSettings,
  SellerEarnings,
  ReferralStats,
  CouponValidationResult,
  BulkImportResult,
  Coupon,
} from "./types";

import { clearAuthSession } from "./authSession";
import { notifyAdsChanged } from "./adsSync";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";
const API_TIMEOUT_MS = 8_000;

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("auth_token");
}

function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("auth_refresh_token");
}

let refreshInFlight: Promise<boolean> | null = null;

async function tryRefreshSession(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const res = await fetch(`${API_URL}/api/auth/refresh`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken }),
        });
        const body = (await res.json()) as ApiResponse<AuthResponse>;
        if (!res.ok || !body.success || !body.data?.token) return false;
        localStorage.setItem("auth_token", body.data.token);
        if (body.data.refreshToken) {
          localStorage.setItem("auth_refresh_token", body.data.refreshToken);
        }
        return true;
      } catch {
        return false;
      } finally {
        refreshInFlight = null;
      }
    })();
  }

  return refreshInFlight;
}

async function parseResponse<T>(res: Response): Promise<ApiResponse<T>> {
  const text = await res.text();
  let body: ApiResponse<T>;

  if (!text.trim()) {
    if (!res.ok) {
      const fallback =
        res.status === 404
          ? "Kaynak bulunamadı. API güncel mi? dotnet run ile API'yi yeniden başlatın."
          : res.statusText || "İstek başarısız";
      throw new ApiError(fallback, res.status);
    }
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

export async function request<T>(
  path: string,
  options: RequestInit = {},
  auth = false,
): Promise<ApiResponse<T>> {
  const headers = new Headers(options.headers);

  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  if (auth) {
    const token = getToken();
    if (token) headers.set("Authorization", `Bearer ${token}`);
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), API_TIMEOUT_MS);

  try {
    const res = await fetch(`${API_URL}${path}`, {
      ...options,
      headers,
      signal: controller.signal,
    });
    try {
      return await parseResponse<T>(res);
    } catch (err) {
      if (
        auth &&
        err instanceof ApiError &&
        err.status === 401
      ) {
        const refreshed = await tryRefreshSession();
        if (refreshed) {
          const retryHeaders = new Headers(options.headers);
          if (!(options.body instanceof FormData)) {
            retryHeaders.set("Content-Type", "application/json");
          }
          const retryToken = getToken();
          if (retryToken) retryHeaders.set("Authorization", `Bearer ${retryToken}`);
          const retryRes = await fetch(`${API_URL}${path}`, {
            ...options,
            headers: retryHeaders,
            signal: controller.signal,
          });
          return await parseResponse<T>(retryRes);
        }
        clearAuthSession();
      } else if (auth && err instanceof ApiError && err.status === 403) {
        clearAuthSession(err.message);
      }
      throw err;
    }
  } catch (err) {
    if (err instanceof DOMException && err.name === "AbortError") {
      throw new ApiError(
        "API yanıt vermiyor. Terminalde: dotnet run --project AdvertisementApp.API",
        0,
      );
    }
    if (err instanceof TypeError) {
      throw new ApiError(
        "API'ye bağlanılamadı. http://localhost:5050 çalışıyor mu?",
        0,
      );
    }
    throw err;
  } finally {
    clearTimeout(timeoutId);
  }
}

async function notifyAdsOnSuccess<T>(call: () => Promise<ApiResponse<T>>) {
  const res = await call();
  if (res.success) notifyAdsChanged();
  return res;
}

export const api = {
  login: (data: LoginRequest) =>
    request<AuthResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  register: (data: RegisterRequest) =>
    request<AuthResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  getAuthPublicConfig: () =>
    request<{
      googleClientId: string;
      emailEnabled: boolean;
      emailUsesPickup: boolean;
      stripePublishableKey: string;
      stripeEnabled: boolean;
      iyzicoEnabled: boolean;
      webPushVapidPublicKey: string;
      captchaSiteKey: string;
      captchaEnabled: boolean;
    }>("/api/auth/public-config"),

  refreshSession: (refreshToken: string) =>
    request<AuthResponse>("/api/auth/refresh", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    }),

  logout: (refreshToken?: string) =>
    request<void>("/api/auth/logout", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    }, true),

  me: () => request<UserProfile>("/api/auth/me", {}, true),

  getCategories: () => request<Category[]>("/api/categories"),
  getCategoryTree: () => request<CategoryTreeNode[]>("/api/categories/tree"),

  getSeoLanding: (city: string, categoryPath?: string) => {
    const q = new URLSearchParams({ city });
    if (categoryPath) q.set("categoryPath", categoryPath);
    return request<import("./types").SeoLanding>(`/api/seo/landing?${q}`);
  },

  getSeoSitemapEntries: (max = 500) =>
    request<{ citySlug: string; categoryPath: string }[]>(
      `/api/seo/sitemap-entries?max=${max}`,
    ),

  getProvinces: () => request<Province[]>("/api/locations/provinces"),
  getDistricts: (provinceId: number) =>
    request<District[]>(`/api/locations/provinces/${provinceId}/districts`),
  getNeighborhoods: (districtId: number) =>
    request<Neighborhood[]>(`/api/locations/districts/${districtId}/neighborhoods`),

  getCargoCarriers: () => request<CargoCarrier[]>("/api/marketplace/orders/carriers"),
  createMarketplaceOrder: (advertisementId: number, paymentMethod: string) =>
    request<MarketplaceOrder>(
      "/api/marketplace/orders",
      { method: "POST", body: JSON.stringify({ advertisementId, paymentMethod }) },
      true,
    ),
  payMarketplaceOrder: (
    orderId: number,
    card?: { cardHolder?: string; cardNumberLast4?: string },
  ) =>
    request<PayMarketplaceOrderResult>(
      `/api/marketplace/orders/${orderId}/pay`,
      { method: "POST", body: JSON.stringify(card ?? {}) },
      true,
    ),
  shipMarketplaceOrder: (orderId: number, carrierCode: string, trackingNumber: string) =>
    request<MarketplaceOrder>(
      `/api/marketplace/orders/${orderId}/ship`,
      { method: "POST", body: JSON.stringify({ carrierCode, trackingNumber }) },
      true,
    ),
  confirmMarketplaceDelivery: (orderId: number) =>
    request<MarketplaceOrder>(`/api/marketplace/orders/${orderId}/confirm-delivery`, { method: "POST" }, true),
  getMarketplaceOrder: (orderId: number) =>
    request<MarketplaceOrder>(`/api/marketplace/orders/${orderId}`, {}, true),
  getMyMarketplaceOrders: (asSeller = false) =>
    request<MarketplaceOrder[]>(`/api/marketplace/orders/mine?asSeller=${asSeller}`, {}, true),
  openMarketplaceDispute: (orderId: number, reason: string) =>
    request<MarketplaceOrder>(
      `/api/marketplace/orders/${orderId}/dispute`,
      { method: "POST", body: JSON.stringify({ reason }) },
      true,
    ),

  getSellerRating: (sellerUserId: number, page = 1, pageSize = 10) =>
    request<SellerRatingSummary>(`/api/reviews/seller/${sellerUserId}?page=${page}&pageSize=${pageSize}`),
  createSellerReview: (sellerUserId: number, marketplaceOrderId: number, rating: number, comment?: string) =>
    request<SellerReview>(
      "/api/reviews",
      {
        method: "POST",
        body: JSON.stringify({ sellerUserId, marketplaceOrderId, rating, comment }),
      },
      true,
    ),
  createBuyerReview: (buyerUserId: number, marketplaceOrderId: number, rating: number, comment?: string) =>
    request<SellerReview>(
      "/api/reviews/buyer",
      {
        method: "POST",
        body: JSON.stringify({ buyerUserId, marketplaceOrderId, rating, comment }),
      },
      true,
    ),
  getAdvertisementRating: (advertisementId: number, page = 1, pageSize = 10) =>
    request<AdvertisementRatingSummary>(
      `/api/reviews/advertisement/${advertisementId}?page=${page}&pageSize=${pageSize}`,
      {},
      true,
    ),
  createAdvertisementReview: (
    advertisementId: number,
    marketplaceOrderId: number,
    rating: number,
    comment?: string,
  ) =>
    request<AdvertisementReview>(
      "/api/reviews/advertisement",
      {
        method: "POST",
        body: JSON.stringify({ advertisementId, marketplaceOrderId, rating, comment }),
      },
      true,
    ),

  sendPhoneVerificationCode: (phoneNumber: string) =>
    request<void>("/api/auth/phone/send-code", { method: "POST", body: JSON.stringify({ phoneNumber }) }, true),
  verifyPhoneCode: (phoneNumber: string, code: string) =>
    request<UserProfile>(
      "/api/auth/phone/verify",
      { method: "POST", body: JSON.stringify({ phoneNumber, code }) },
      true,
    ),

  getAdvertisements: (filter: AdvertisementFilter = {}) => {
    const params = new URLSearchParams();
    if (filter.search) params.set("search", filter.search);
    if (filter.categoryId) params.set("categoryId", String(filter.categoryId));
    if (filter.sellerUserId) params.set("sellerUserId", String(filter.sellerUserId));
    if (filter.minPrice != null) params.set("minPrice", String(filter.minPrice));
    if (filter.maxPrice != null) params.set("maxPrice", String(filter.maxPrice));
    if (filter.city) params.set("city", filter.city);
    if (filter.featuredOnly) params.set("featuredOnly", "true");
    if (filter.brand) params.set("brand", filter.brand);
    if (filter.model) params.set("model", filter.model);
    if (filter.minYear != null) params.set("minYear", String(filter.minYear));
    if (filter.maxYear != null) params.set("maxYear", String(filter.maxYear));
    if (filter.minMileage != null) params.set("minMileage", String(filter.minMileage));
    if (filter.maxMileage != null) params.set("maxMileage", String(filter.maxMileage));
    if (filter.listingId != null) params.set("listingId", String(filter.listingId));
    if (filter.status !== undefined)
      params.set("status", String(filter.status));
    params.set("page", String(filter.page ?? 1));
    params.set("pageSize", String(filter.pageSize ?? 12));
    const qs = params.toString();
    return request<PagedResult<Advertisement>>(
      `/api/advertisements${qs ? `?${qs}` : ""}`,
    );
  },

  getMyAdvertisements: (filter: AdvertisementFilter = {}) => {
    const params = new URLSearchParams();
    params.set("mine", "true");
    if (filter.search) params.set("search", filter.search);
    if (filter.categoryId) params.set("categoryId", String(filter.categoryId));
    if (filter.status !== undefined) params.set("status", String(filter.status));
    params.set("page", String(filter.page ?? 1));
    params.set("pageSize", String(filter.pageSize ?? 12));
    const qs = params.toString();
    return request<PagedResult<Advertisement>>(
      `/api/advertisements?${qs}`,
      {},
      true,
    );
  },

  getMyAdCounts: () => request<MyAdCounts>("/api/advertisements/my/counts", {}, true),

  revealPhone: (advertisementId: number) =>
    request<PhoneReveal>(`/api/advertisements/${advertisementId}/phone`, {}, true),

  bumpAdvertisement: (id: number) =>
    notifyAdsOnSuccess(() =>
      request<void>(`/api/advertisements/${id}/bump`, { method: "POST" }, true),
    ),

  extendAdvertisement: (id: number, days = 30) =>
    notifyAdsOnSuccess(() =>
      request<void>(`/api/advertisements/${id}/extend?days=${days}`, { method: "POST" }, true),
    ),

  getSellerAnalytics: (userId: number) =>
    request<SellerAnalytics>(`/api/sellers/${userId}/analytics`),

  getAdvertisement: (id: number) =>
    request<Advertisement>(`/api/advertisements/${id}`),

  getSimilarAdvertisements: (id: number, count = 4) =>
    request<Advertisement[]>(`/api/advertisements/${id}/similar?count=${count}`),

  getPriceHistory: (id: number) =>
    request<PriceHistoryPoint[]>(`/api/advertisements/${id}/price-history`),

  getAdvertisementsBatch: (ids: number[]) => {
    if (ids.length === 0) return Promise.resolve({ success: true, message: "", data: [] as Advertisement[] });
    return request<Advertisement[]>(`/api/advertisements/batch?ids=${ids.join(",")}`);
  },

  getSellerProfile: (userId: number) =>
    request<SellerProfile>(`/api/sellers/${userId}`),

  getSellerBySlug: (slug: string) =>
    request<SellerProfile>(`/api/sellers/by-slug/${encodeURIComponent(slug)}`),

  getAdPackages: (variant?: string) => {
    const q = variant ? `?variant=${encodeURIComponent(variant)}` : "";
    return request<PackageExperiment>(`/api/payments/packages${q}`);
  },

  logPackageExperiment: (data: { variant: string; event: string; packageId?: number }) =>
    request<void>("/api/payments/packages/experiment-log", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  forgotPassword: (email: string) =>
    request<void>("/api/auth/forgot-password", {
      method: "POST",
      body: JSON.stringify({ email }),
    }),

  resetPassword: (data: {
    email: string;
    token: string;
    newPassword: string;
    confirmPassword: string;
  }) =>
    request<void>("/api/auth/reset-password", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  recordAdView: (id: number) =>
    request<void>(`/api/advertisements/${id}/view`, { method: "POST" }),

  createAdvertisement: (data: AdvertisementCreateInput) =>
    notifyAdsOnSuccess(() =>
      request<Advertisement>("/api/advertisements", {
        method: "POST",
        body: JSON.stringify(data),
      }, true),
    ),

  createAdvertisementWithImage: (formData: FormData) =>
    notifyAdsOnSuccess(() =>
      request<Advertisement>("/api/advertisements/with-image", {
        method: "POST",
        body: formData,
      }, true),
    ),

  createAdvertisementWithImages: (formData: FormData) =>
    notifyAdsOnSuccess(() =>
      request<Advertisement>("/api/advertisements/with-images", {
        method: "POST",
        body: formData,
      }, true),
    ),

  updateAdvertisement: (id: number, data: AdvertisementUpdateInput) =>
    request<Advertisement>(`/api/advertisements/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    }, true),

  updateAdvertisementWithImage: (id: number, formData: FormData) =>
    request<Advertisement>(`/api/advertisements/${id}/with-images`, {
      method: "PUT",
      body: formData,
    }, true),

  deleteAdvertisement: (id: number) =>
    notifyAdsOnSuccess(() =>
      request<void>(`/api/advertisements/${id}`, { method: "DELETE" }, true),
    ),

  getFavorites: () =>
    request<Advertisement[]>("/api/favorites", {}, true),

  getFavoriteStatus: (advertisementId: number) =>
    request<boolean>(`/api/favorites/${advertisementId}/status`, {}, true),

  toggleFavorite: (advertisementId: number) =>
    request<boolean>(`/api/favorites/${advertisementId}/toggle`, { method: "POST" }, true),

  updateProfile: (data: {
    firstName: string;
    lastName: string;
    email: string;
  }) =>
    request<UserProfile>("/api/auth/profile", {
      method: "PUT",
      body: JSON.stringify(data),
    }, true),

  changePassword: (data: {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
  }) =>
    request<void>("/api/auth/change-password", {
      method: "POST",
      body: JSON.stringify(data),
    }, true),

  uploadProfilePhoto: (formData: FormData) =>
    request<UserProfile>("/api/auth/profile-photo", {
      method: "POST",
      body: formData,
    }, true),

  removeProfilePhoto: () =>
    request<UserProfile>("/api/auth/profile-photo", { method: "DELETE" }, true),

  externalLogin: (provider: string, idToken: string, email?: string, firstName?: string, lastName?: string) =>
    request<AuthResponse>("/api/auth/external", {
      method: "POST",
      body: JSON.stringify({ provider, idToken, email, firstName, lastName }),
    }),

  getMessageThreads: () => request<MessageThread[]>("/api/messages/threads", {}, true),
  getThreadMessages: (threadId: number) =>
    request<ChatMessage[]>(`/api/messages/threads/${threadId}`, {}, true),
  sendMessage: (advertisementId: number, body: string, threadId?: number) =>
    threadId && threadId > 0
      ? request<ChatMessage>(
          `/api/messages/threads/${threadId}`,
          { method: "POST", body: JSON.stringify({ body }) },
          true,
        )
      : request<ChatMessage>(
          "/api/messages",
          { method: "POST", body: JSON.stringify({ advertisementId, body }) },
          true,
        ),

  createOffer: (advertisementId: number, amount: number, message?: string) =>
    request<Offer>("/api/offers", {
      method: "POST",
      body: JSON.stringify({ advertisementId, amount, message }),
    }, true),
  getOffers: (advertisementId: number, asOwner = false) =>
    request<Offer[]>(`/api/offers/ad/${advertisementId}?asOwner=${asOwner}`, {}, true),
  getIncomingOffers: () => request<Offer[]>("/api/offers/incoming", {}, true),
  respondToOffer: (offerId: number, accept: boolean) =>
    request<Offer>(`/api/offers/${offerId}/${accept ? "accept" : "reject"}`, { method: "POST" }, true),

  getBlogPosts: () => request<PublicBlogPost[]>("/api/content/blog"),
  getBlogPost: (slug: string) =>
    request<PublicBlogPost>(`/api/content/blog/${encodeURIComponent(slug)}`),
  getStaticPages: () => request<PublicStaticPageListItem[]>("/api/content/pages"),
  getStaticPage: (slug: string) =>
    request<PublicStaticPage>(`/api/content/pages/${encodeURIComponent(slug)}`),

  reportListing: (advertisementId: number, reason: string, details?: string) =>
    request<void>("/api/reports", {
      method: "POST",
      body: JSON.stringify({ advertisementId, reason, details }),
    }, true),

  registerPushSubscription: (endpoint: string, p256dh: string, auth: string) =>
    request<void>(
      "/api/push/subscribe",
      {
        method: "POST",
        body: JSON.stringify({ endpoint, p256dh, auth }),
      },
      true,
    ),

  getNotifications: () => request<AppNotification[]>("/api/notifications", {}, true),
  getUnreadNotificationCount: () =>
    request<number>("/api/notifications/unread-count", {}, true),
  markNotificationRead: (id: number) =>
    request<void>(`/api/notifications/${id}/read`, { method: "POST" }, true),

  logSearch: (categoryId?: number, search?: string) => {
    const p = new URLSearchParams();
    if (categoryId) p.set("categoryId", String(categoryId));
    if (search) p.set("search", search);
    return request<void>(`/api/analytics/search-log?${p}`, { method: "POST" });
  },
  getAnalyticsOverview: () => request<AnalyticsOverview>("/api/analytics/overview"),
  getAdAnalytics: (id: number) =>
    request<AdAnalytics>(`/api/analytics/ad/${id}`, {}, true),
  getMapListings: (params: { categoryId?: number; search?: string; city?: string }) => {
    const p = new URLSearchParams();
    if (params.categoryId) p.set("categoryId", String(params.categoryId));
    if (params.search) p.set("search", params.search);
    if (params.city) p.set("city", params.city);
    return request<MapListing[]>(`/api/analytics/map?${p}`);
  },

  startCheckout: (adPackageId: number, advertisementId: number, couponCode?: string) =>
    request<CheckoutResult>(
      "/api/payments/checkout",
      {
        method: "POST",
        body: JSON.stringify({ adPackageId, advertisementId, couponCode: couponCode || undefined }),
      },
      true,
    ),
  completeCheckout: (purchaseId: number) =>
    request<void>(`/api/payments/complete/${purchaseId}`, { method: "POST" }, true),

  completeStripeSession: (sessionId: string) =>
    request<void>(
      `/api/payments/stripe-session?sessionId=${encodeURIComponent(sessionId)}`,
      { method: "POST" },
      true,
    ),

  setFavoritePriceAlert: (advertisementId: number, priceAlertEnabled: boolean, alertPrice?: number) =>
    request<void>(
      `/api/favorites/${advertisementId}/price-alert`,
      {
        method: "PUT",
        body: JSON.stringify({ priceAlertEnabled, alertPrice }),
      },
      true,
    ),

  getAuctionByAd: (advertisementId: number) =>
    request<AuctionInfo>(`/api/auctions/by-ad/${advertisementId}`),
  createAuction: (data: {
    advertisementId: number;
    startsAt: string;
    endsAt: string;
    startingBid: number;
    minIncrement: number;
  }) =>
    request<AuctionInfo>("/api/auctions", { method: "POST", body: JSON.stringify(data) }, true),
  placeAuctionBid: (auctionId: number, amount: number) =>
    request<AuctionInfo>(
      `/api/auctions/${auctionId}/bid`,
      { method: "POST", body: JSON.stringify({ amount }) },
      true,
    ),

  queryTramer: (plate: string, chassisNumber?: string) =>
    request<TramerResult>(
      "/api/tramer/query",
      { method: "POST", body: JSON.stringify({ plate, chassisNumber }) },
      true,
    ),
  saveTramerToAd: (advertisementId: number, plate: string, chassisNumber?: string) =>
    request<TramerResult>(
      `/api/tramer/save/${advertisementId}`,
      { method: "POST", body: JSON.stringify({ plate, chassisNumber }) },
      true,
    ),

  getSavedSearches: () => request<SavedSearch[]>("/api/saved-searches", {}, true),
  createSavedSearch: (data: { name: string; filterJson: string; notifyOnNew?: boolean }) =>
    request<SavedSearch>("/api/saved-searches", { method: "POST", body: JSON.stringify(data) }, true),
  deleteSavedSearch: (id: number) =>
    request<void>(`/api/saved-searches/${id}`, { method: "DELETE" }, true),

  getMyVerification: () => request<VerificationRequest | null>("/api/verification/mine", {}, true),
  submitVerification: (documentType: string, file: File) => {
    const form = new FormData();
    form.append("documentType", documentType);
    form.append("document", file);
    return request<VerificationRequest>("/api/verification/submit", { method: "POST", body: form }, true);
  },

  getListingQuestions: (advertisementId: number) =>
    request<ListingQuestion[]>(`/api/growth/listing-questions/${advertisementId}`),
  askListingQuestion: (advertisementId: number, question: string) =>
    request<ListingQuestion>(
      "/api/growth/listing-questions",
      { method: "POST", body: JSON.stringify({ advertisementId, question }) },
      true,
    ),
  answerListingQuestion: (questionId: number, answer: string) =>
    request<ListingQuestion>(
      `/api/growth/listing-questions/${questionId}/answer`,
      { method: "POST", body: JSON.stringify({ answer }) },
      true,
    ),

  followSeller: (sellerUserId: number) =>
    request<void>(`/api/growth/follow/${sellerUserId}`, { method: "POST" }, true),
  unfollowSeller: (sellerUserId: number) =>
    request<void>(`/api/growth/follow/${sellerUserId}`, { method: "DELETE" }, true),
  isFollowingSeller: (sellerUserId: number) =>
    request<boolean>(`/api/growth/follow/${sellerUserId}/status`, {}, true),
  getFollowedSellers: () => request<SellerFollow[]>(`/api/growth/followed-sellers`, {}, true),

  updateStoreSettings: (data: UpdateStoreSettings) =>
    request<void>("/api/growth/store", { method: "PUT", body: JSON.stringify(data) }, true),
  getStoreSettings: () => request<UpdateStoreSettings>("/api/growth/store", {}, true),
  getSellerEarnings: () => request<SellerEarnings>("/api/growth/earnings", {}, true),
  getReferralStats: () => request<ReferralStats>("/api/growth/referral", {}, true),
  validateCoupon: (code: string, orderAmount: number) =>
    request<CouponValidationResult>(
      "/api/growth/coupons/validate",
      { method: "POST", body: JSON.stringify({ code, orderAmount }) },
    ),
  importSellerCsv: (csv: string) =>
    request<BulkImportResult>("/api/growth/import/csv", { method: "POST", body: JSON.stringify(csv) }, true),
};

/** API ayakta mı — SignalR / polling öncesi hızlı kontrol */
export async function isApiReachable(timeoutMs = 2500): Promise<boolean> {
  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);
    const res = await fetch(`${API_URL}/api/auth/public-config`, { signal: controller.signal });
    clearTimeout(timeoutId);
    return res.ok;
  } catch {
    return false;
  }
}

export { API_URL };
