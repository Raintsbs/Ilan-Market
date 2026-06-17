"use client";

import { useEffect, useMemo, useState } from "react";
import { SafeImage } from "@/components/SafeImage";
import { api, ApiError } from "@/lib/api";
import { CategoryTreePicker } from "@/components/CategoryTreePicker";
import { ListingDetailsForm } from "@/components/ListingDetailsForm";
import { validateTitleCategory } from "@/lib/categoryTitleValidation";
import { getImageUrl } from "@/lib/image";
import { inferListingType, isBrandModelRequired, resolveCategoryProfile, buildCategoryPath } from "@/lib/categoryProfile";
import {
  EMPTY_LISTING_DETAILS,
  normalizeListingDetails,
  parseListingDetails,
  serializeListingDetails,
  type ListingDetails,
} from "@/lib/listingDetails";
import { useLocale } from "@/context/LocaleContext";
import { TurnstileWidget } from "@/components/TurnstileWidget";
import type { Advertisement, Category } from "@/lib/types";
import {
  alertErrorClass,
  alertWarningClass,
  formFieldClass,
  formFileClass,
} from "@/lib/formStyles";

const MAX_IMAGES = 10;
const MAX_SIZE_MB = 5;
const MAX_VIDEO_MB = 80;

interface AdFormProps {
  initial?: Advertisement;
  onSuccess: (id: number, message?: string) => void;
}

export function AdForm({ initial, onSuccess }: AdFormProps) {
  const { t } = useLocale();
  const isEdit = !!initial;
  const [categories, setCategories] = useState<Category[]>([]);
  const [categoryId, setCategoryId] = useState(String(initial?.categoryId ?? ""));
  const [title, setTitle] = useState(initial?.title ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [content, setContent] = useState(initial?.content ?? "");
  const [images, setImages] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [keptPaths, setKeptPaths] = useState<string[]>([]);
  const [listingDetails, setListingDetails] = useState<ListingDetails>(() =>
    initial ? normalizeListingDetails(parseListingDetails(initial.listingDetails)) : { ...EMPTY_LISTING_DETAILS },
  );
  const [categoryWarning, setCategoryWarning] = useState<string | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [captchaSiteKey, setCaptchaSiteKey] = useState("");
  const [captchaEnabled, setCaptchaEnabled] = useState(false);
  const [captchaToken, setCaptchaToken] = useState<string | null>(null);
  const [video, setVideo] = useState<File | null>(null);
  const [panorama, setPanorama] = useState<File | null>(null);
  const [auctionEnabled, setAuctionEnabled] = useState(false);
  const [auctionStarts, setAuctionStarts] = useState("");
  const [auctionEnds, setAuctionEnds] = useState("");
  const [auctionStartingBid, setAuctionStartingBid] = useState("");
  const [auctionMinIncrement, setAuctionMinIncrement] = useState("100");

  const selectedCategory = useMemo(
    () => categories.find((c) => String(c.id) === categoryId),
    [categories, categoryId],
  );

  const selectedCategoryPath = useMemo(
    () => buildCategoryPath(categories, categoryId),
    [categories, categoryId],
  );

  const categoryProfile = useMemo(
    () => resolveCategoryProfile(selectedCategory?.name, selectedCategoryPath),
    [selectedCategory?.name, selectedCategoryPath],
  );

  useEffect(() => {
    if (isEdit) return;
    api.getAuthPublicConfig().then((res) => {
      if (res.success && res.data) {
        setCaptchaSiteKey(res.data.captchaSiteKey ?? "");
        setCaptchaEnabled(!!res.data.captchaEnabled);
      }
    });
  }, [isEdit]);

  useEffect(() => {
    api.getCategories().then((res) => {
      if (res.success && res.data) setCategories(res.data.filter((c) => c.isActive));
    });
  }, []);

  useEffect(() => {
    if (initial?.auction) {
      setAuctionEnabled(true);
      return;
    }
    if (!auctionStarts) {
      const start = new Date();
      const end = new Date(start.getTime() + 7 * 24 * 60 * 60 * 1000);
      setAuctionStarts(start.toISOString().slice(0, 16));
      setAuctionEnds(end.toISOString().slice(0, 16));
    }
  }, [initial?.auction, auctionStarts]);

  useEffect(() => {
    if (!selectedCategoryPath) return;
    const parts = selectedCategoryPath.split(" › ");
    if (parts.length < 3) return;

    const brand = parts.length >= 4 ? parts[parts.length - 2] : parts[parts.length - 1];
    const model = parts.length >= 4 ? parts[parts.length - 1] : "";

    setListingDetails((prev) => ({
      ...prev,
      brand: prev.brand.trim() ? prev.brand : brand,
      model: prev.model.trim() ? prev.model : model,
    }));
  }, [categoryId, selectedCategoryPath]);

  useEffect(() => {
    if (!initial) return;
    if (initial.imagePaths?.length) {
      setKeptPaths([...initial.imagePaths]);
    } else if (initial.imagePath) {
      setKeptPaths([initial.imagePath]);
    } else {
      setKeptPaths([]);
    }
    const parsed = parseListingDetails(initial.listingDetails);
    if (parsed.sellerType === "Sahibinden") parsed.sellerType = "Bireysel";
    setListingDetails(normalizeListingDetails(parsed));
  }, [initial]);

  function validateListingDetails(): string | null {
    if (!categoryId) return t("form.pickCategory");
    const profile = resolveCategoryProfile(selectedCategory?.name, selectedCategoryPath);
    if (!profile) return t("form.pickCategory");

    if (listingDetails.price == null || listingDetails.price <= 0) {
      return t("form.priceRequired");
    }
    if (!listingDetails.city.trim() || !listingDetails.district.trim()) {
      return t("form.locationRequired");
    }
    if (isBrandModelRequired(profile)) {
      if (!listingDetails.brand.trim() || !listingDetails.model.trim()) {
        return t("form.brandRequired");
      }
    }
    if (!profile.showJob && !profile.showService && !listingDetails.condition.trim()) {
      return t("form.conditionRequired");
    }
    if (auctionEnabled) {
      if (!auctionStarts || !auctionEnds) return t("auction.notStarted");
      if (Number(auctionStartingBid) <= 0) return t("form.priceRequired");
      if (Number(auctionMinIncrement) <= 0) return t("form.priceRequired");
    }
    return null;
  }

  function resolveListingType(): number {
    if (auctionEnabled) return 2;
    return inferListingType(categoryProfile);
  }

  function appendListingJson(formData: FormData) {
    formData.append("ListingDetailsJson", serializeListingDetails(listingDetails));
  }

  function appendExtendedFields(formData: FormData) {
    formData.append("ListingType", String(resolveListingType()));
  }

  function buildFormData(): FormData {
    const formData = new FormData();
    const catId = Number(categoryId);
    formData.append("CategoryId", String(catId));
    formData.append("Title", title.trim());
    formData.append("Description", description.trim());
    formData.append("Content", content.trim());
    appendListingJson(formData);
    appendExtendedFields(formData);
    if (isEdit) {
      formData.append("ImagePathsJson", JSON.stringify(keptPaths));
      if (keptPaths[0]) formData.append("ImagePath", keptPaths[0]);
    }
    images.forEach((file) => formData.append("images", file));
    if (video) formData.append("video", video);
    if (panorama) formData.append("panorama", panorama);
    if (captchaToken) formData.append("CaptchaToken", captchaToken);
    return formData;
  }

  async function maybeCreateAuction(adId: number) {
    if (!auctionEnabled || initial?.auction) return;
    const res = await api.createAuction({
      advertisementId: adId,
      startsAt: new Date(auctionStarts).toISOString(),
      endsAt: new Date(auctionEnds).toISOString(),
      startingBid: Number(auctionStartingBid),
      minIncrement: Number(auctionMinIncrement),
    });
    if (!res.success) throw new Error(res.message);
  }

  useEffect(() => {
    if (selectedCategory && title.trim()) {
      setCategoryWarning(validateTitleCategory(title, selectedCategory.name));
    } else {
      setCategoryWarning(null);
    }
  }, [title, selectedCategory]);

  function handleCategoryChange(value: string) {
    setCategoryId(value);
    const cat = categories.find((c) => String(c.id) === value);
    if (cat && title.trim()) {
      const warn = validateTitleCategory(title, cat.name);
      setCategoryWarning(warn);
    } else {
      setCategoryWarning(null);
    }
  }

  function handleImagesChange(files: FileList | null) {
    if (!files?.length) return;
    const list: File[] = [];
    const urls: string[] = [];

    for (let i = 0; i < files.length && list.length < MAX_IMAGES; i++) {
      const f = files[i];
      if (f.size > MAX_SIZE_MB * 1024 * 1024) {
        setError(t("form.imageMaxSize", { mb: MAX_SIZE_MB }));
        continue;
      }
      list.push(f);
      urls.push(URL.createObjectURL(f));
    }

    setImages((prev) => [...prev, ...list].slice(0, MAX_IMAGES));
    setPreviews((prev) => [...prev, ...urls].slice(0, MAX_IMAGES));
    setError("");
  }

  function removeImage(idx: number) {
    setImages((prev) => prev.filter((_, i) => i !== idx));
    setPreviews((prev) => {
      URL.revokeObjectURL(prev[idx]);
      return prev.filter((_, i) => i !== idx);
    });
  }

  function removeKeptPath(idx: number) {
    setKeptPaths((prev) => prev.filter((_, i) => i !== idx));
  }

  const keptPreviews = useMemo(
    () =>
      keptPaths
        .map((p) => ({ path: p, url: getImageUrl(p) }))
        .filter((x): x is { path: string; url: string } => !!x.url),
    [keptPaths],
  );

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    const detailsError = validateListingDetails();
    if (detailsError) {
      setError(detailsError);
      return;
    }
    if (!isEdit && captchaEnabled && !captchaToken) {
      setError("Lütfen güvenlik doğrulamasını tamamlayın.");
      return;
    }

    setLoading(true);
    const listingJson = serializeListingDetails(listingDetails);

    try {
      const catId = Number(categoryId);
      if (!catId || !title.trim() || !description.trim() || !content.trim()) {
        throw new Error(t("form.required"));
      }

      const listingType = resolveListingType();
      const needsMultipart = images.length > 0 || !!video || !!panorama;

      if (isEdit) {
        if (needsMultipart) {
          const res = await api.updateAdvertisementWithImage(initial!.id, buildFormData());
          if (!res.success) throw new Error(res.message);
          const adId = res.data?.id ?? initial!.id;
          await maybeCreateAuction(adId);
          onSuccess(adId, res.message);
          return;
        }

        const res = await api.updateAdvertisement(initial!.id, {
          categoryId: catId,
          title: title.trim(),
          description: description.trim(),
          content: content.trim(),
          imagePath: keptPaths[0],
          imagePathsJson: JSON.stringify(keptPaths),
          listingDetailsJson: listingJson,
          listingType,
        });
        if (!res.success) throw new Error(res.message);
        const adId = res.data?.id ?? initial!.id;
        await maybeCreateAuction(adId);
        onSuccess(adId, res.message);
        return;
      }

      if (needsMultipart) {
        const res = await api.createAdvertisementWithImages(buildFormData());
        if (!res.success) throw new Error(res.message);
        const adId = res.data?.id ?? 0;
        if (adId) await maybeCreateAuction(adId);
        onSuccess(adId, res.message);
        return;
      }

      const res = await api.createAdvertisement({
        categoryId: catId,
        title: title.trim(),
        description: description.trim(),
        content: content.trim(),
        listingDetailsJson: listingJson,
        listingType,
        captchaToken: captchaToken ?? undefined,
      });
      if (!res.success) throw new Error(res.message);
      const adId = res.data?.id ?? 0;
      if (adId) await maybeCreateAuction(adId);
      onSuccess(adId, res.message);
    } catch (err) {
      setError(err instanceof ApiError || err instanceof Error ? err.message : t("account.error"));
      setCaptchaToken(null);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {error && (
        <div className={alertErrorClass}>{error}</div>
      )}

      {categoryWarning && (
        <div className={alertWarningClass}>
          <strong>{t("common.warning")}:</strong> {categoryWarning}
        </div>
      )}

      <div className="grid gap-6 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("form.category")} *</label>
          <CategoryTreePicker
            required
            value={categoryId}
            onChange={handleCategoryChange}
          />
        </div>

        <div className="sm:col-span-2">
          <ListingDetailsForm
            value={listingDetails}
            onChange={setListingDetails}
            categoryName={selectedCategory?.name}
            categoryPath={selectedCategoryPath}
            advertisementId={isEdit ? initial?.id : undefined}
          />
        </div>

        {categoryProfile && !categoryProfile.showJob && !categoryProfile.showService && (
          <div className="sm:col-span-2 space-y-3 rounded-xl border border-amber-200 bg-amber-50/50 p-4 dark:border-amber-900 dark:bg-amber-950/30">
            <label className="flex items-center gap-2 text-sm font-medium text-slate-800 dark:text-slate-200">
              <input
                type="checkbox"
                checked={auctionEnabled}
                onChange={(e) => setAuctionEnabled(e.target.checked)}
                disabled={!!initial?.auction}
              />
              {t("auction.enable")}
            </label>
            {auctionEnabled && !initial?.auction && (
              <div className="grid gap-3 sm:grid-cols-2">
                <div>
                  <label className="mb-1 block text-xs text-slate-600">{t("auction.startsAt")}</label>
                  <input type="datetime-local" value={auctionStarts} onChange={(e) => setAuctionStarts(e.target.value)} className={formFieldClass} />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-slate-600">{t("auction.endsAt")}</label>
                  <input type="datetime-local" value={auctionEnds} onChange={(e) => setAuctionEnds(e.target.value)} className={formFieldClass} />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-slate-600">{t("auction.startingBid")}</label>
                  <input type="number" min={1} value={auctionStartingBid} onChange={(e) => setAuctionStartingBid(e.target.value)} className={formFieldClass} />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-slate-600">{t("auction.minIncrement")}</label>
                  <input type="number" min={1} value={auctionMinIncrement} onChange={(e) => setAuctionMinIncrement(e.target.value)} className={formFieldClass} />
                </div>
              </div>
            )}
          </div>
        )}

        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("form.title")} *</label>
          <input
            required
            maxLength={200}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className={formFieldClass}
          />
        </div>

        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
            {t("form.images")} ({t("form.imagesHint", { max: MAX_IMAGES, mb: MAX_SIZE_MB })})
          </label>
          <input
            type="file"
            multiple
            accept="image/jpeg,image/png,image/gif,image/webp"
            onChange={(e) => {
              handleImagesChange(e.target.files);
              e.target.value = "";
            }}
            className={formFileClass}
          />
          {(previews.length > 0 || keptPreviews.length > 0) && (
            <div className="mt-3 flex flex-wrap gap-2">
              {keptPreviews.map((item, i) => (
                <div
                  key={item.path}
                  className="relative h-20 w-28 overflow-hidden rounded-lg border border-slate-200 dark:border-slate-600"
                >
                  <SafeImage src={item.url} alt="" fill className="object-cover" />
                  <button
                    type="button"
                    onClick={() => removeKeptPath(i)}
                    className="absolute right-1 top-1 rounded bg-rose-600 px-1.5 text-xs text-white"
                    title={t("form.removeImage")}
                  >
                    ×
                  </button>
                  <span className="absolute bottom-0 left-0 right-0 bg-black/50 text-center text-[10px] text-white">
                    {t("form.existing")}
                  </span>
                </div>
              ))}
              {previews.map((url, i) => (
                <div key={url} className="relative h-20 w-28 overflow-hidden rounded-lg border border-blue-200 dark:border-blue-800">
                  <SafeImage src={url} alt="" fill className="object-cover" />
                  <button
                    type="button"
                    onClick={() => removeImage(i)}
                    className="absolute right-1 top-1 rounded bg-rose-600 px-1.5 text-xs text-white"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {(categoryProfile?.showVehicle || categoryProfile?.showEstate) && (
          <>
            <div className="sm:col-span-2">
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
                {t("media.uploadVideo")}
              </label>
              <input
                type="file"
                accept="video/mp4,video/webm,video/quicktime"
                onChange={(e) => {
                  const f = e.target.files?.[0];
                  if (f && f.size > MAX_VIDEO_MB * 1024 * 1024) {
                    setError(`Video en fazla ${MAX_VIDEO_MB} MB olabilir.`);
                    return;
                  }
                  setVideo(f ?? null);
                  e.target.value = "";
                }}
                className={formFileClass}
              />
              {(video || initial?.videoPath) && (
                <p className="mt-1 text-xs text-slate-500">
                  {video?.name ?? initial?.videoPath}
                </p>
              )}
            </div>
            <div className="sm:col-span-2">
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
                {t("media.uploadPanorama")}
              </label>
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                onChange={(e) => {
                  setPanorama(e.target.files?.[0] ?? null);
                  e.target.value = "";
                }}
                className={formFileClass}
              />
              {(panorama || initial?.panoramaPath) && (
                <p className="mt-1 text-xs text-slate-500">
                  {panorama?.name ?? initial?.panoramaPath}
                </p>
              )}
            </div>
          </>
        )}

        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("form.shortDesc")} *</label>
          <textarea
            required
            rows={2}
            maxLength={500}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className={formFieldClass}
          />
          <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
            {description.length}/500
          </p>
        </div>

        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
            {t("form.longDesc")} *
          </label>
          <textarea
            required
            rows={8}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder={t("form.longDescPlaceholder")}
            className={formFieldClass}
          />
        </div>

      </div>

      {!isEdit && captchaEnabled && captchaSiteKey && (
        <TurnstileWidget siteKey={captchaSiteKey} onToken={setCaptchaToken} theme="auto" />
      )}

      <button
        type="submit"
        disabled={loading}
        className="btn-brand w-full rounded-xl bg-slate-900 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-slate-800 disabled:opacity-60 dark:bg-blue-600 dark:hover:bg-blue-500 sm:w-auto sm:px-10"
      >
        {loading ? t("form.saving") : isEdit ? t("form.update") : t("form.publish")}
      </button>
    </form>
  );
}
