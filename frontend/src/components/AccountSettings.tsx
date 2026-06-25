"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { surfaceCardPad } from "@/lib/uiStyles";
import { getImageUrl } from "@/lib/image";
import { SafeImage } from "./SafeImage";

export function AccountSettings() {
  const { user, refreshProfile } = useAuth();
  const { t } = useLocale();
  const { showToast } = useToast();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [profileMsg, setProfileMsg] = useState("");
  const [passwordMsg, setPasswordMsg] = useState("");
  const [photoMsg, setPhotoMsg] = useState("");
  const [loadingProfile, setLoadingProfile] = useState(false);
  const [loadingPassword, setLoadingPassword] = useState(false);
  const [loadingPhoto, setLoadingPhoto] = useState(false);
  const [removingPhoto, setRemovingPhoto] = useState(false);
  const [phoneNumber, setPhoneNumber] = useState("");
  const [phoneCode, setPhoneCode] = useState("");
  const [phoneMsg, setPhoneMsg] = useState("");
  const [sendingCode, setSendingCode] = useState(false);
  const [verifyingPhone, setVerifyingPhone] = useState(false);

  useEffect(() => {
    if (!user) return;
    setFirstName(user.firstName);
    setLastName(user.lastName);
    setEmail(user.email);
    setPhoneNumber(user.phoneNumber ?? "");
  }, [user]);

  const avatarUrl = user?.profileImagePath ? getImageUrl(user.profileImagePath) : null;

  async function handleProfileSave(e: React.FormEvent) {
    e.preventDefault();
    setProfileMsg("");
    setLoadingProfile(true);
    try {
      const res = await api.updateProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
      });
      if (!res.success) throw new Error(res.message);
      await refreshProfile();
      setProfileMsg(t("account.profileSaved"));
    } catch (err) {
      setProfileMsg(err instanceof ApiError || err instanceof Error ? err.message : t("account.error"));
    } finally {
      setLoadingProfile(false);
    }
  }

  async function handlePasswordSave(e: React.FormEvent) {
    e.preventDefault();
    setPasswordMsg("");
    setLoadingPassword(true);
    try {
      const res = await api.changePassword({
        currentPassword,
        newPassword,
        confirmPassword,
      });
      if (!res.success) throw new Error(res.message);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      setPasswordMsg(t("account.passwordSaved"));
    } catch (err) {
      setPasswordMsg(err instanceof ApiError || err instanceof Error ? err.message : t("account.error"));
    } finally {
      setLoadingPassword(false);
    }
  }

  async function handleRemovePhoto() {
    if (!avatarUrl || !confirm(t("confirm.removePhoto"))) return;
    setPhotoMsg("");
    setRemovingPhoto(true);
    try {
      const res = await api.removeProfilePhoto();
      if (!res.success) throw new Error(res.message);
      await refreshProfile();
      showToast(t("toast.photoRemoved"), "success");
    } catch (err) {
      const msg = err instanceof ApiError || err instanceof Error ? err.message : t("toast.photoRemoveError");
      setPhotoMsg(msg);
      showToast(msg, "error");
    } finally {
      setRemovingPhoto(false);
    }
  }

  async function handleSendPhoneCode() {
    const normalized = phoneNumber.trim();
    if (!normalized) return;
    setPhoneMsg("");
    setSendingCode(true);
    try {
      const res = await api.sendPhoneVerificationCode(normalized);
      if (!res.success) throw new Error(res.message);
      setPhoneMsg(t("phone.codeSent"));
      showToast(t("phone.codeSent"), "success");
    } catch (err) {
      const msg = err instanceof ApiError || err instanceof Error ? err.message : t("phone.verifyFailed");
      setPhoneMsg(msg);
      showToast(msg, "error");
    } finally {
      setSendingCode(false);
    }
  }

  async function handleVerifyPhone(e: React.FormEvent) {
    e.preventDefault();
    setPhoneMsg("");
    setVerifyingPhone(true);
    try {
      const res = await api.verifyPhoneCode(phoneNumber.trim(), phoneCode.trim());
      if (!res.success) throw new Error(res.message);
      await refreshProfile();
      setPhoneCode("");
      setPhoneMsg(t("phone.verified"));
      showToast(t("phone.verified"), "success");
    } catch (err) {
      const msg = err instanceof ApiError || err instanceof Error ? err.message : t("phone.verifyFailed");
      setPhoneMsg(msg);
      showToast(msg, "error");
    } finally {
      setVerifyingPhone(false);
    }
  }

  async function handlePhotoChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setPhotoMsg("");
    setLoadingPhoto(true);
    try {
      const fd = new FormData();
      fd.append("image", file);
      const res = await api.uploadProfilePhoto(fd);
      if (!res.success) throw new Error(res.message);
      await refreshProfile();
      showToast(t("toast.photoUpdated"), "success");
    } catch (err) {
      const msg = err instanceof ApiError || err instanceof Error ? err.message : t("account.error");
      setPhotoMsg(msg);
      showToast(msg, "error");
    } finally {
      setLoadingPhoto(false);
      e.target.value = "";
    }
  }

  return (
    <div className="mt-8 space-y-6">
      <section id="profile-photo" className={`${surfaceCardPad} shadow-sm`}>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{t("account.profilePhoto")}</h3>
        <div className="mt-4 flex flex-wrap items-center gap-4">
          <label className="group relative h-20 w-20 cursor-pointer overflow-hidden rounded-2xl bg-slate-100">
            {avatarUrl ? (
              <SafeImage src={avatarUrl} alt="Profil" fill className="object-cover" />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-2xl font-bold text-blue-600">
                {(firstName?.charAt(0) || email?.charAt(0) || "?").toUpperCase()}
              </div>
            )}
            <span className="avatar-edit-overlay absolute inset-0 flex items-center justify-center bg-black/50 text-xs font-semibold text-white">
              {t("account.editAvatar")}
            </span>
            <input
              type="file"
              accept="image/jpeg,image/png,image/gif,image/webp"
              className="hidden"
              disabled={loadingPhoto || removingPhoto}
              onChange={handlePhotoChange}
            />
          </label>
          <div className="flex flex-wrap gap-2">
            <label className="cursor-pointer rounded-xl border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 dark:border-slate-600 dark:text-slate-200 dark:hover:bg-slate-800">
              {loadingPhoto ? t("account.uploading") : t("account.choosePhoto")}
              <input
                type="file"
                accept="image/jpeg,image/png,image/gif,image/webp"
                className="hidden"
                disabled={loadingPhoto || removingPhoto}
                onChange={handlePhotoChange}
              />
            </label>
            {avatarUrl && (
              <button
                type="button"
                onClick={handleRemovePhoto}
                disabled={loadingPhoto || removingPhoto}
                className="rounded-xl border border-rose-200 px-4 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 disabled:opacity-60 dark:border-rose-800 dark:text-rose-400 dark:hover:bg-rose-950"
              >
                {removingPhoto ? t("account.removingPhoto") : t("account.removePhoto")}
              </button>
            )}
          </div>
        </div>
        {photoMsg && (
          <p className="mt-3 text-sm text-rose-600 dark:text-rose-400">{photoMsg}</p>
        )}
      </section>

      <section className={`${surfaceCardPad} shadow-sm`}>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{t("account.personalInfo")}</h3>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t("account.personalHint")}</p>
        <form onSubmit={handleProfileSave} className="mt-4 space-y-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("auth.firstName")}</label>
              <input
                required
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                className={formFieldClass}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("auth.lastName")}</label>
              <input
                required
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                className={formFieldClass}
              />
            </div>
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("account.emailUsername")}</label>
            <input
              required
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={formFieldClass}
            />
          </div>
          {profileMsg && (
            <p className={`text-sm ${profileMsg === t("account.profileSaved") ? "text-emerald-600" : "text-rose-600"}`}>
              {profileMsg}
            </p>
          )}
          <button
            type="submit"
            disabled={loadingProfile}
            className="rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {loadingProfile ? t("account.saving") : t("account.save")}
          </button>
        </form>
      </section>

      <section className={`${surfaceCardPad} shadow-sm`}>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{t("phone.verifyTitle")}</h3>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t("phone.verifyHint")}</p>
        {user?.phoneVerified ? (
          <p className="mt-4 text-sm font-medium text-emerald-600 dark:text-emerald-400">
            ✓ {t("phone.verified")}: {user.phoneNumber}
          </p>
        ) : (
          <form onSubmit={handleVerifyPhone} className="mt-4 space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("phone.number")}</label>
              <div className="flex flex-col gap-2 sm:flex-row sm:flex-wrap">
                <input
                  required
                  type="tel"
                  value={phoneNumber}
                  onChange={(e) => setPhoneNumber(e.target.value)}
                  placeholder="05xx xxx xx xx"
                  className={`min-w-0 w-full flex-1 sm:min-w-[12rem] ${formFieldClass}`}
                />
                <button
                  type="button"
                  disabled={sendingCode || !phoneNumber.trim()}
                  onClick={handleSendPhoneCode}
                  className="min-h-10 w-full rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60 sm:w-auto dark:border-slate-600 dark:text-slate-200"
                >
                  {sendingCode ? "…" : t("phone.sendCode")}
                </button>
              </div>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("phone.code")}</label>
              <input
                required
                value={phoneCode}
                onChange={(e) => setPhoneCode(e.target.value)}
                maxLength={6}
                className={formFieldClass}
              />
            </div>
            {phoneMsg && (
              <p className={`text-sm ${phoneMsg === t("phone.verified") || phoneMsg === t("phone.codeSent") ? "text-emerald-600" : "text-rose-600"}`}>
                {phoneMsg}
              </p>
            )}
            <button
              type="submit"
              disabled={verifyingPhone}
              className="rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-60"
            >
              {verifyingPhone ? "…" : t("phone.verify")}
            </button>
          </form>
        )}
      </section>

      <section className={`${surfaceCardPad} shadow-sm`}>
        <h3 className="text-lg font-semibold text-slate-900 dark:text-white">{t("account.changePassword")}</h3>
        <form onSubmit={handlePasswordSave} className="mt-4 space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("account.currentPassword")}</label>
            <input
              required
              type="password"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              className={formFieldClass}
            />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("account.newPassword")}</label>
              <input
                required
                type="password"
                minLength={6}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className={formFieldClass}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">{t("account.confirmPassword")}</label>
              <input
                required
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className={formFieldClass}
              />
            </div>
          </div>
          {passwordMsg && (
            <p className={`text-sm ${passwordMsg === t("account.passwordSaved") ? "text-emerald-600" : "text-rose-600"}`}>
              {passwordMsg}
            </p>
          )}
          <button
            type="submit"
            disabled={loadingPassword}
            className="rounded-xl border border-slate-200 px-6 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
          >
            {loadingPassword ? t("account.updating") : t("account.updatePassword")}
          </button>
        </form>
      </section>
    </div>
  );
}
