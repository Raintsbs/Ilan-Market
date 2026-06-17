"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/status";
import { PageHeader } from "@/components/PageHeader";
import { listItemLink, pageContainerMd } from "@/lib/uiStyles";
import type { PublicBlogPost } from "@/lib/types";

export default function BlogListPage() {
  const { t, locale } = useLocale();
  const [posts, setPosts] = useState<PublicBlogPost[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getBlogPosts().then((res) => {
      if (res.success && res.data) setPosts(res.data);
      setLoading(false);
    });
  }, []);

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("blog.title")} subtitle={t("blog.subtitle")} />

      {loading ? (
        <div className="mt-10">
          <LoadingSpinner />
        </div>
      ) : posts.length === 0 ? (
        <p className="mt-10 text-center text-slate-500">{t("blog.empty")}</p>
      ) : (
        <ul className="mt-8 space-y-4">
          {posts.map((post) => (
            <li key={post.id}>
              <Link
                href={`/blog/${post.slug}`}
                className={`${listItemLink} p-5`}
              >
                <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{post.title}</h2>
                {post.summary && (
                  <p className="mt-2 line-clamp-2 text-sm text-slate-600 dark:text-slate-400">{post.summary}</p>
                )}
                <p className="mt-3 text-xs text-slate-400">
                  {formatDate(post.publishedTime ?? post.createdTime, locale)}
                </p>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
