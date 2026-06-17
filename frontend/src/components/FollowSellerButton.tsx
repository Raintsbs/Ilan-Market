"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { UserPlus, UserMinus } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";

export function FollowSellerButton({ sellerUserId, isSelf }: { sellerUserId: number; isSelf?: boolean }) {
  const { isAuthenticated } = useAuth();
  const { t } = useLocale();
  const router = useRouter();
  const [following, setFollowing] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated || isSelf) {
      setLoading(false);
      return;
    }
    api.isFollowingSeller(sellerUserId).then((r) => {
      if (r.success && r.data != null) setFollowing(r.data);
      setLoading(false);
    });
  }, [isAuthenticated, isSelf, sellerUserId]);

  if (isSelf || loading) return null;

  async function toggle() {
    if (!isAuthenticated) {
      router.push(`/giris?redirect=/satici/${sellerUserId}`);
      return;
    }
    if (following) {
      const res = await api.unfollowSeller(sellerUserId);
      if (res.success) setFollowing(false);
    } else {
      const res = await api.followSeller(sellerUserId);
      if (res.success) setFollowing(true);
    }
  }

  return (
    <button
      type="button"
      onClick={toggle}
      className="inline-flex items-center gap-2 rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-800"
    >
      {following ? <UserMinus className="size-4" /> : <UserPlus className="size-4" />}
      {following ? t("follow.unfollow") : t("follow.follow")}
    </button>
  );
}
