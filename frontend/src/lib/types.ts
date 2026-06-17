export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export enum AdvertisementStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

export interface ListingDetails {
  price?: number | null;
  city?: string;
  district?: string;
  neighborhood?: string;
  condition?: string;
  brand?: string;
  model?: string;
  year?: string;
  warranty?: string;
  sellerType?: string;
  swap?: boolean;
  color?: string;
  storage?: string;
  memory?: string;
  processor?: string;
  screenSize?: string;
  mileage?: string;
  fuelType?: string;
  transmission?: string;
  damageStatus?: string;
  tramerStatus?: string;
  expertReportUrl?: string;
  damageRecord?: string;
  floorPlanUrl?: string;
  deedStatus?: string;
  rentalYield?: string;
  videoUrl?: string;
  virtualTourUrl?: string;
  roomCount?: string;
  squareMeters?: string;
  buildingAge?: string;
  floor?: string;
  heating?: string;
  coffeeType?: string;
  capacity?: string;
  powerWatts?: string;
  batteryHealth?: string;
  material?: string;
  size?: string;
  salaryMin?: string;
  salaryMax?: string;
  employmentType?: string;
  experienceLevel?: string;
  workMode?: string;
  serviceType?: string;
  serviceArea?: string;
  priceUnit?: string;
}

export type ListingType = "standard" | "auction" | "job" | "service";

export interface TramerResult {
  plate: string;
  status: string;
  damageCount: number;
  totalDamageAmount?: number | null;
  summary?: string;
  queriedAt: string;
  isSimulated: boolean;
}

export interface AuctionBid {
  id: number;
  userId: number;
  userDisplayName?: string;
  amount: number;
  createdTime: string;
}

export interface AuctionInfo {
  id: number;
  advertisementId: number;
  startsAt: string;
  endsAt: string;
  startingBid: number;
  minIncrement: number;
  currentBid?: number | null;
  highBidderUserId?: number | null;
  status: string;
  bidCount: number;
  recentBids: AuctionBid[];
}

export interface Advertisement {
  id: number;
  userId: number;
  categoryId: number;
  categoryName: string;
  userDisplayName?: string;
  title: string;
  description: string;
  imagePath?: string;
  imagePaths?: string[];
  videoPath?: string | null;
  panoramaPath?: string | null;
  listingType?: ListingType | number;
  content: string;
  listingDetails?: ListingDetails | null;
  tramerResult?: TramerResult | null;
  auction?: AuctionInfo | null;
  isActive: boolean;
  status: AdvertisementStatus;
  createdTime: string;
  updatedTime?: string;
  viewCount?: number;
  isFeatured?: boolean;
  sellerIsVerified?: boolean;
  sellerAverageRating?: number;
  sellerReviewCount?: number;
  averageRating?: number;
  reviewCount?: number;
  rejectReason?: string;
}

export interface AdvertisementFilter {
  search?: string;
  categoryId?: number;
  sellerUserId?: number;
  status?: AdvertisementStatus;
  page?: number;
  pageSize?: number;
  minPrice?: number;
  maxPrice?: number;
  city?: string;
  featuredOnly?: boolean;
  brand?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minMileage?: number;
  maxMileage?: number;
  listingId?: number;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  parentId?: number | null;
  sortOrder?: number;
  createdTime: string;
  updatedTime?: string;
  fieldSchemaJson?: string;
  slug?: string;
}

export interface SavedSearch {
  id: number;
  name: string;
  filterJson: string;
  notifyOnNew: boolean;
  createdTime: string;
}

export interface PayMarketplaceOrderResult {
  order?: MarketplaceOrder | null;
  stripeCheckoutUrl?: string | null;
  checkoutUrl?: string | null;
  paymentProvider?: string | null;
  isDemo: boolean;
  message?: string | null;
}

export interface SeoLanding {
  citySlug: string;
  cityName: string;
  categoryId?: number | null;
  categoryName?: string | null;
  categoryPath?: string | null;
  breadcrumbs: { name: string; slug: string; path: string }[];
  totalCount: number;
  shouldIndex: boolean;
}

export interface CategoryTreeNode extends Category {
  children?: CategoryTreeNode[];
}

export interface MyAdCounts {
  all: number;
  pending: number;
  approved: number;
  rejected: number;
}

export interface PhoneReveal {
  phoneNumber: string;
  displayName: string;
}

export interface CheckoutResult {
  purchaseId: number;
  checkoutUrl: string;
  message: string;
  isDemo?: boolean;
  stripeSessionId?: string;
  paymentProvider?: string | null;
}

export interface SellerAnalytics {
  userId: number;
  displayName: string;
  activeListingCount: number;
  totalViews: number;
  totalOffers: number;
  totalMessageThreads: number;
  topAds: SellerAdStat[];
}

export interface SellerAdStat {
  advertisementId: number;
  title: string;
  viewCount: number;
  offerCount: number;
  messageThreadCount: number;
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  expiresAt?: string;
  userId: number;
  email: string;
  roles: string[];
}

export interface UserProfile {
  userId: number;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  profileImagePath?: string;
  roles: string[];
  isVerified?: boolean;
  phoneNumber?: string;
  phoneVerified?: boolean;
}

export interface Province {
  id: number;
  name: string;
  plateCode?: string;
}

export interface District {
  id: number;
  provinceId: number;
  name: string;
}

export interface Neighborhood {
  id: number;
  districtId: number;
  name: string;
}

export interface MarketplaceOrder {
  id: number;
  advertisementId: number;
  advertisementTitle?: string;
  buyerUserId: number;
  sellerUserId: number;
  amount: number;
  paymentMethod: string;
  status: number;
  statusLabel: string;
  createdTime: string;
  paidAt?: string;
  completedAt?: string;
  shipment?: OrderShipment;
  canReview?: boolean;
  canReviewAd?: boolean;
  canReviewBuyer?: boolean;
  canOpenDispute?: boolean;
  disputeReason?: string | null;
  disputedAt?: string | null;
  disputeResolutionNote?: string | null;
  cancelledAt?: string | null;
  refundedAt?: string | null;
  refundNote?: string | null;
  sellerPaidOutAt?: string | null;
  sellerPayoutNote?: string | null;
}

export interface OrderShipment {
  id: number;
  carrierCode: string;
  carrierName: string;
  trackingNumber: string;
  trackingUrl?: string;
  status: string;
  shippedAt?: string;
  deliveredAt?: string;
}

export interface CargoCarrier {
  code: string;
  name: string;
}

export interface SellerReview {
  id: number;
  sellerUserId: number;
  buyerName?: string;
  marketplaceOrderId?: number;
  rating: number;
  comment?: string;
  isVerifiedPurchase?: boolean;
  createdTime: string;
}

export interface AdvertisementReview {
  id: number;
  advertisementId: number;
  userName?: string;
  marketplaceOrderId?: number;
  rating: number;
  comment?: string;
  isVerifiedPurchase?: boolean;
  createdTime: string;
}

export interface AdvertisementRatingSummary {
  averageRating: number;
  reviewCount: number;
  reviews: AdvertisementReview[];
  page: number;
  pageSize: number;
  totalPages: number;
  canReview: boolean;
  reviewOrderId?: number;
  alreadyReviewed?: boolean;
}

export interface PriceHistoryPoint {
  price: number;
  recordedAt: string;
}

export interface SellerRatingSummary {
  averageRating: number;
  reviewCount: number;
  recentReviews: SellerReview[];
  page: number;
  pageSize: number;
  totalPages: number;
  canReview?: boolean;
  reviewOrderId?: number;
  alreadyReviewed?: boolean;
}

export interface MessageThread {
  id: number;
  advertisementId: number;
  advertisementTitle: string;
  otherUserId: number;
  otherUserName: string;
  lastMessage?: string;
  lastMessageTime?: string;
  unreadCount: number;
}

export interface ChatMessage {
  id: number;
  threadId: number;
  senderUserId: number;
  isMine: boolean;
  body: string;
  createdTime: string;
}

export interface Offer {
  id: number;
  advertisementId: number;
  advertisementTitle?: string;
  buyerUserId: number;
  buyerName?: string;
  amount: number;
  message?: string;
  status: number;
  createdTime: string;
  messageThreadId?: number;
}

export interface PublicBlogPost {
  id: number;
  title: string;
  slug: string;
  summary?: string;
  content?: string;
  publishedTime?: string;
  createdTime: string;
}

export interface PublicStaticPage {
  slug: string;
  title: string;
  content: string;
  updatedTime: string;
}

export interface PublicStaticPageListItem {
  slug: string;
  title: string;
}

export interface SellerProfile {
  userId: number;
  displayName: string;
  isVerified: boolean;
  activeListingCount: number;
  totalViews: number;
  memberSince: string;
  profileImagePath?: string;
  storeSlug?: string;
  companyName?: string;
  storeDescription?: string;
  storeBannerPath?: string;
  isCorporateStore?: boolean;
  completedOrderCount?: number;
  averageRating?: number;
  reviewCount?: number;
}

export interface PackageExperiment {
  variant: string;
  packages: AdPackage[];
}

export interface AppNotification {
  id: number;
  type: string;
  title: string;
  body: string;
  link?: string;
  isRead: boolean;
  createdTime: string;
}

export interface AdAnalytics {
  advertisementId: number;
  viewCount: number;
  offerCount: number;
  messageThreadCount: number;
}

export interface AnalyticsOverview {
  topCategories: { categoryId?: number; categoryName: string; searchCount: number }[];
  totalSearchesLast7Days: number;
}

export interface MapListing {
  id: number;
  title: string;
  price?: number;
  city: string;
  district?: string;
  lat: number;
  lng: number;
  imagePath?: string;
}

export interface AdPackage {
  id: number;
  code: string;
  name: string;
  price: number;
  featuredDays: number;
}

export interface AdminDashboard {
  totalAds: number;
  pendingAds: number;
  totalUsers: number;
  openReports: number;
  featuredAds: number;
  expiredAds: number;
  categoryDistribution: CategoryAdCount[];
}

export interface CategoryAdCount {
  categoryId: number;
  categoryName: string;
  count: number;
}

export interface AuditLog {
  id: number;
  actorEmail: string;
  action: string;
  details?: string;
  createdTime: string;
}

export interface AdminReviewItem {
  id: number;
  reviewType: string;
  rating: number;
  comment?: string;
  isHidden: boolean;
  createdTime: string;
  authorName?: string;
  targetName?: string;
  advertisementId?: number;
  advertisementTitle?: string;
}

export interface ReportItem {
  id: number;
  advertisementId: number;
  adTitle: string;
  reporterUserId: number;
  reason: string;
  details?: string;
  status: string;
  createdTime: string;
}

export interface AdminUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isVerified: boolean;
  isBanned: boolean;
  isFrozen: boolean;
  warningCount: number;
  phoneVerified: boolean;
  phoneNumber?: string;
  adCount: number;
}

export interface AdminUserDetail extends AdminUser {
  banReason?: string;
  frozenUntil?: string;
  recentAds: Advertisement[];
  activity: { id: number; type: string; message: string; createdTime: string }[];
}

export interface AdminCategory {
  id: number;
  name: string;
  description?: string;
  parentId?: number;
  parentName?: string;
  slug?: string;
  sortOrder: number;
  fieldSchemaJson?: string;
  isActive: boolean;
  adCount: number;
  childCount?: number;
}

export interface StaticPage {
  id: number;
  slug: string;
  title: string;
  content: string;
  isActive: boolean;
}

export interface CityLocation {
  id: number;
  city: string;
  district?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface BlogPost {
  id: number;
  title: string;
  slug: string;
  summary?: string;
  content: string;
  isPublished: boolean;
  createdTime: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  captchaToken?: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  captchaToken?: string;
  referralCode?: string;
}

export interface AdminAdPackage {
  id: number;
  code: string;
  name: string;
  price: number;
  featuredDays: number;
  isActive: boolean;
}

export interface AdvertisementCreateInput {
  categoryId: number;
  title: string;
  description: string;
  content: string;
  imagePath?: string;
  listingDetailsJson?: string;
  listingType?: number;
  videoPath?: string | null;
  panoramaPath?: string | null;
  captchaToken?: string;
}

export interface AdvertisementUpdateInput extends AdvertisementCreateInput {
  /** Yalnızca admin; kullanıcı düzenlemesinde gönderilmez. */
  isActive?: boolean;
  imagePathsJson?: string;
}

export interface VerificationRequest {
  id: number;
  userId: number;
  userEmail?: string;
  documentType: string;
  filePath: string;
  status: string;
  adminNote?: string;
  createdAt: string;
}

export interface ListingQuestion {
  id: number;
  advertisementId: number;
  userId: number;
  userName?: string;
  question: string;
  answer?: string;
  createdTime: string;
  answeredTime?: string;
  isOwnerAnswer?: boolean;
}

export interface SellerFollow {
  sellerUserId: number;
  displayName: string;
  storeSlug?: string;
  activeListingCount: number;
  followedAt: string;
}

export interface UpdateStoreSettings {
  storeSlug?: string;
  companyName?: string;
  storeDescription?: string;
  isCorporateStore?: boolean;
}

export interface SellerEarnings {
  totalCompletedAmount: number;
  pendingPayoutAmount: number;
  paidOutAmount: number;
  completedOrderCount: number;
}

export interface ReferralStats {
  referralCode: string;
  referredUserCount: number;
  shareUrl: string;
}

export interface CouponValidationResult {
  valid: boolean;
  discountAmount: number;
  message?: string;
}

export interface BulkImportResult {
  created: number;
  failed: number;
  errors: string[];
}

export interface Coupon {
  id: number;
  code: string;
  description?: string;
  discountAmount: number;
  discountPercent?: number;
  maxUses: number;
  usedCount: number;
  expiresAt?: string;
  isActive: boolean;
}
