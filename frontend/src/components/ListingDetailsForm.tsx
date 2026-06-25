"use client";

import { useMemo } from "react";
import type { ListingDetails } from "@/lib/listingDetails";
import {
  COFFEE_TYPES,
  CONDITIONS,
  DEED_STATUSES,
  EMPLOYMENT_TYPES,
  EXPERIENCE_LEVELS,
  FUEL_TYPES,
  normalizeListingDetails,
  PRICE_UNITS,
  SELLER_TYPES,
  TRAMER_STATUSES,
  TRANSMISSIONS,
  WARRANTIES,
  WORK_MODES,
} from "@/lib/listingDetails";
import { TramerQueryPanel } from "@/components/TramerQueryPanel";
import { LocationSelector } from "@/components/LocationSelector";
import { useLocale } from "@/context/LocaleContext";
import { isBrandModelRequired, resolveCategoryProfile } from "@/lib/categoryProfile";
import {
  alertHintClass,
  alertInfoClass,
  formFieldClass as inputClass,
} from "@/lib/formStyles";

interface ListingDetailsFormProps {
  value: ListingDetails;
  onChange: (next: ListingDetails) => void;
  categoryName?: string;
  categoryPath?: string;
  advertisementId?: number;
}

export function ListingDetailsForm({ value, onChange, categoryName, categoryPath, advertisementId }: ListingDetailsFormProps) {
  const { t } = useLocale();
  const profile = useMemo(() => resolveCategoryProfile(categoryName, categoryPath), [categoryName, categoryPath]);
  const brandRequired = isBrandModelRequired(profile);

  function set<K extends keyof ListingDetails>(key: K, val: ListingDetails[K]) {
    onChange(normalizeListingDetails({ ...value, [key]: val }));
  }

  if (!profile) {
    return (
      <div className={alertHintClass}>{t("form.pickCategoryFirst")}</div>
    );
  }

  return (
    <div className="space-y-8">
      <div className={alertInfoClass}>
        <span className="font-medium">{profile.label}</span> {t("form.categoryFieldsHint")}
      </div>

      <section>
        <h3 className="text-base font-semibold text-slate-900 dark:text-white">{t("listing.priceLocation")}</h3>
        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.price")} *</label>
            <input
              required
              type="number"
              min={0}
              step={1}
              value={value.price ?? ""}
              onChange={(e) =>
                set("price", e.target.value === "" ? null : Number(e.target.value))
              }
              placeholder="Örn. 25000"
              className={inputClass}
            />
          </div>
          <div className="sm:col-span-2">
            <LocationSelector
              required
              value={{
                city: value.city ?? "",
                district: value.district ?? "",
                neighborhood: value.neighborhood ?? "",
              }}
              onChange={(loc) =>
                onChange(
                  normalizeListingDetails({
                    ...value,
                    city: loc.city ?? "",
                    district: loc.district ?? "",
                    neighborhood: loc.neighborhood ?? "",
                  }),
                )
              }
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.seller")} *</label>
            <select
              required
              value={value.sellerType}
              onChange={(e) => set("sellerType", e.target.value)}
              className={inputClass}
            >
              {SELLER_TYPES.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </div>
        </div>
      </section>

      <section>
        <h3 className="text-base font-semibold text-slate-900">{profile.productSectionTitle}</h3>
        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700">
              {t("listing.brand")} {brandRequired ? "*" : ""}
            </label>
            <input
              required={brandRequired}
              value={value.brand}
              onChange={(e) => set("brand", e.target.value)}
              placeholder={profile.brandPlaceholder}
              className={inputClass}
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700">
              {t("listing.model")} {brandRequired ? "*" : ""}
            </label>
            <input
              required={brandRequired}
              value={value.model}
              onChange={(e) => set("model", e.target.value)}
              placeholder={profile.modelPlaceholder}
              className={inputClass}
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.condition")} *</label>
            <select
              required
              value={value.condition}
              onChange={(e) => set("condition", e.target.value)}
              className={inputClass}
            >
              <option value="">{t("listing.select")}</option>
              {CONDITIONS.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </div>
          {profile.showYear && (
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.year")}</label>
              <input
                value={value.year}
                onChange={(e) => set("year", e.target.value)}
                placeholder="Örn. 2024"
                className={inputClass}
              />
            </div>
          )}
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.warranty")}</label>
            <select
              value={value.warranty}
              onChange={(e) => set("warranty", e.target.value)}
              className={inputClass}
            >
              <option value="">{t("listing.select")}</option>
              {WARRANTIES.map((w) => (
                <option key={w} value={w}>
                  {w}
                </option>
              ))}
            </select>
          </div>
          {profile.showColor && (
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("listing.color")}</label>
              <input
                value={value.color}
                onChange={(e) => set("color", e.target.value)}
                className={inputClass}
              />
            </div>
          )}
          {profile.showSwap && (
            <div className="flex items-center gap-2 sm:col-span-2">
              <input
                id="swap"
                type="checkbox"
                checked={value.swap}
                onChange={(e) => set("swap", e.target.checked)}
                className="h-4 w-4 rounded border-slate-300 text-blue-600"
              />
              <label htmlFor="swap" className="text-sm text-slate-700">
                {t("listing.swap")}
              </label>
            </div>
          )}
        </div>
      </section>

      {profile.showComputer && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Bilgisayar özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.storage")} value={value.storage} onChange={(v) => set("storage", v)} placeholder="512 GB SSD" />
            <Field label={t("row.ram")} value={value.memory} onChange={(v) => set("memory", v)} placeholder="16 GB" />
            <Field label={t("row.processor")} value={value.processor} onChange={(v) => set("processor", v)} placeholder="Intel i7 / Ryzen 7" />
            <Field label={t("row.screen")} value={value.screenSize} onChange={(v) => set("screenSize", v)} placeholder='15.6"' />
          </div>
        </section>
      )}

      {(profile.showPhone || profile.showTablet) && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">
            {profile.showPhone ? "Telefon özellikleri" : "Tablet özellikleri"}
          </h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Depolama" value={value.storage} onChange={(v) => set("storage", v)} placeholder="128 GB" />
            <Field label="Ekran boyutu" value={value.screenSize} onChange={(v) => set("screenSize", v)} placeholder='6.7"' />
            {profile.showPhone && (
              <Field
                label="Pil sağlığı"
                value={value.batteryHealth}
                onChange={(v) => set("batteryHealth", v)}
                placeholder="%85"
              />
            )}
          </div>
        </section>
      )}

      {profile.showCoffee && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Kahve makinesi özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.coffeeType")}</label>
              <select
                value={value.coffeeType}
                onChange={(e) => set("coffeeType", e.target.value)}
                className={inputClass}
              >
                <option value="">{t("listing.select")}</option>
                {COFFEE_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </select>
            </div>
            <Field
              label={t("row.capacity")}
              value={value.capacity}
              onChange={(v) => set("capacity", v)}
              placeholder="1.8 litre"
            />
            <Field
              label={t("row.pressure")}
              value={value.processor}
              onChange={(v) => set("processor", v)}
              placeholder="15 bar"
            />
            <Field
              label={t("row.power")}
              value={value.powerWatts}
              onChange={(v) => set("powerWatts", v)}
              placeholder="1450 W"
            />
          </div>
        </section>
      )}

      {profile.showTv && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Televizyon özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Ekran boyutu" value={value.screenSize} onChange={(v) => set("screenSize", v)} placeholder='55"' />
            <Field label={t("row.resolution")} value={value.storage} onChange={(v) => set("storage", v)} placeholder="4K UHD" />
          </div>
        </section>
      )}

      {profile.showGaming && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Konsol / oyun özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Depolama" value={value.storage} onChange={(v) => set("storage", v)} placeholder="825 GB" />
            <Field label="Paket içeriği" value={value.memory} onChange={(v) => set("memory", v)} placeholder="2 kol, oyun dahil" />
          </div>
        </section>
      )}

      {profile.showJob && (
        <section>
          <h3 className="text-base font-semibold text-slate-900 dark:text-white">{t("row.employmentType")}</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.salary") + " (min)"} value={value.salaryMin} onChange={(v) => set("salaryMin", v)} placeholder="25.000 TL" />
            <Field label={t("row.salary") + " (max)"} value={value.salaryMax} onChange={(v) => set("salaryMax", v)} placeholder="40.000 TL" />
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.employmentType")}</label>
              <select value={value.employmentType} onChange={(e) => set("employmentType", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {EMPLOYMENT_TYPES.map((x) => (
                  <option key={x} value={x}>{x}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.experienceLevel")}</label>
              <select value={value.experienceLevel} onChange={(e) => set("experienceLevel", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {EXPERIENCE_LEVELS.map((x) => (
                  <option key={x} value={x}>{x}</option>
                ))}
              </select>
            </div>
            <div className="sm:col-span-2">
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.workMode")}</label>
              <select value={value.workMode} onChange={(e) => set("workMode", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {WORK_MODES.map((x) => (
                  <option key={x} value={x}>{x}</option>
                ))}
              </select>
            </div>
          </div>
        </section>
      )}

      {profile.showService && (
        <section>
          <h3 className="text-base font-semibold text-slate-900 dark:text-white">{t("row.serviceType")}</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.serviceType")} value={value.serviceType} onChange={(v) => set("serviceType", v)} placeholder="Tadilat, nakliye..." />
            <Field label={t("row.serviceArea")} value={value.serviceArea} onChange={(v) => set("serviceArea", v)} placeholder="İstanbul Avrupa" />
            <div className="sm:col-span-2">
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.priceUnit")}</label>
              <select value={value.priceUnit} onChange={(e) => set("priceUnit", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {PRICE_UNITS.map((x) => (
                  <option key={x} value={x}>{x}</option>
                ))}
              </select>
            </div>
          </div>
        </section>
      )}

      {profile.showVehicle && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Araç özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.mileage")} value={value.mileage} onChange={(v) => set("mileage", v)} placeholder="125.000 km" />
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.fuel")}</label>
              <select value={value.fuelType} onChange={(e) => set("fuelType", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {FUEL_TYPES.map((f) => (
                  <option key={f} value={f}>{f}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.transmission")}</label>
              <select value={value.transmission} onChange={(e) => set("transmission", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {TRANSMISSIONS.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <Field label={t("row.damage")} value={value.damageStatus} onChange={(v) => set("damageStatus", v)} placeholder="Hasarsız" />
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.tramer")}</label>
              <select value={value.tramerStatus} onChange={(e) => set("tramerStatus", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {TRAMER_STATUSES.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>
            <Field
              label={t("row.damageRecord")}
              value={value.damageRecord}
              onChange={(v) => set("damageRecord", v)}
              placeholder="Örn. Sol ön çamurluk boyalı"
              className="sm:col-span-2"
            />
            <Field
              label={t("row.expertReport")}
              value={value.expertReportUrl}
              onChange={(v) => set("expertReportUrl", v)}
              placeholder="https://... ekspertiz PDF veya rapor linki"
              className="sm:col-span-2"
            />
          </div>
          {advertisementId && (
            <div className="mt-6">
              <TramerQueryPanel advertisementId={advertisementId} />
            </div>
          )}
        </section>
      )}

      {profile.showEstate && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Konut özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.rooms")} value={value.roomCount} onChange={(v) => set("roomCount", v)} placeholder="3+1" />
            <Field label={t("row.sqm")} value={value.squareMeters} onChange={(v) => set("squareMeters", v)} />
            <Field label={t("row.buildingAge")} value={value.buildingAge} onChange={(v) => set("buildingAge", v)} />
            <Field label={t("row.floor")} value={value.floor} onChange={(v) => set("floor", v)} />
            <Field label={t("row.heating")} value={value.heating} onChange={(v) => set("heating", v)} placeholder="Doğalgaz kombi" className="sm:col-span-2" />
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("row.deedStatus")}</label>
              <select value={value.deedStatus} onChange={(e) => set("deedStatus", e.target.value)} className={inputClass}>
                <option value="">{t("listing.select")}</option>
                {DEED_STATUSES.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>
            <Field
              label={t("row.rentalYield")}
              value={value.rentalYield}
              onChange={(v) => set("rentalYield", v)}
              placeholder="Örn. %5,2 veya 18.000 TL/ay"
            />
            <Field
              label={t("row.floorPlan")}
              value={value.floorPlanUrl}
              onChange={(v) => set("floorPlanUrl", v)}
              placeholder="Kat planı PDF veya görsel linki"
              className="sm:col-span-2"
            />
          </div>
        </section>
      )}

      {(profile.showVehicle || profile.showEstate) && (
        <section>
          <h3 className="text-base font-semibold text-slate-900 dark:text-white">{t("media.sectionTitle")}</h3>
          <p className="mt-1 text-sm text-slate-500">{t("media.sectionHint")}</p>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field
              label={t("row.video")}
              value={value.videoUrl}
              onChange={(v) => set("videoUrl", v)}
              placeholder="YouTube video linki"
            />
            <Field
              label={t("row.virtualTour")}
              value={value.virtualTourUrl}
              onChange={(v) => set("virtualTourUrl", v)}
              placeholder="Matterport / 360° tur linki"
            />
          </div>
        </section>
      )}

      {profile.showAppliance && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Ev aleti özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Kapasite" value={value.capacity} onChange={(v) => set("capacity", v)} placeholder="9 kg" />
            <Field label="Güç (W)" value={value.powerWatts} onChange={(v) => set("powerWatts", v)} />
            <Field label="Enerji sınıfı" value={value.storage} onChange={(v) => set("storage", v)} placeholder="A++" />
          </div>
        </section>
      )}

      {profile.showFurniture && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Mobilya özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("row.material")} value={value.material} onChange={(v) => set("material", v)} placeholder="Ahşap, kumaş" />
            <Field label={t("row.size")} value={value.size} onChange={(v) => set("size", v)} placeholder="200x90 cm" />
          </div>
        </section>
      )}

      {profile.showClothing && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Giyim özellikleri</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Beden" value={value.size} onChange={(v) => set("size", v)} placeholder="M, 42, 38" />
            <Field label="Malzeme" value={value.material} onChange={(v) => set("material", v)} />
          </div>
        </section>
      )}

      {profile.kind === "electronics" && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Ek bilgiler</h3>
          <p className="mt-1 text-sm text-slate-500">İsteğe bağlı teknik detaylar</p>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Depolama / kapasite" value={value.storage} onChange={(v) => set("storage", v)} />
            <Field label="Ek özellik" value={value.processor} onChange={(v) => set("processor", v)} />
          </div>
        </section>
      )}

      {profile.kind === "other" && (
        <section>
          <h3 className="text-base font-semibold text-slate-900">Ek özellikler</h3>
          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Ek bilgi 1" value={value.storage} onChange={(v) => set("storage", v)} />
            <Field label="Ek bilgi 2" value={value.processor} onChange={(v) => set("processor", v)} />
          </div>
        </section>
      )}
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
  placeholder,
  className = "",
}: {
  label: string;
  value: string | undefined;
  onChange: (v: string) => void;
  placeholder?: string;
  className?: string;
}) {
  return (
    <div className={className}>
      <label className="mb-1.5 block text-sm font-medium text-slate-700">{label}</label>
      <input
        value={value ?? ""}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className={inputClass}
      />
    </div>
  );
}
