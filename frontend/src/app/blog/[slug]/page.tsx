"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { ContentHtml } from "@/components/ContentHtml";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/status";
import type { PublicBlogPost } from "@/lib/types";

export default function BlogDetailPage() {
  const params = useParams();
  const slug = String(params.slug ?? "");
  const { t, locale } = useLocale();
  const [post, setPost] = useState<PublicBlogPost | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!slug) return;
    api.getBlogPost(slug).then((res) => {
      if (res.success && res.data) setPost(res.data);
      else setError(res.message || t("blog.notFound"));
      setLoading(false);
    });
  }, [slug, t]);

  if (loading) return <LoadingSpinner />;
  if (error || !post?.content) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-slate-600 dark:text-slate-400">{error || t("blog.notFound")}</p>
        <Link href="/blog" className="mt-4 inline-block text-blue-600 hover:underline">
          ← {t("blog.back")}
        </Link>
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <Link href="/blog" className="text-sm font-medium text-blue-600 hover:underline dark:text-blue-400">
        ← {t("blog.back")}
      </Link>
      <h1 className="mt-4 text-3xl font-bold text-slate-900 dark:text-white">{post.title}</h1>
      <p className="mt-2 text-sm text-slate-500">
        {formatDate(post.publishedTime ?? post.createdTime, locale)}
      </p>
      {post.summary && (
        <p className="mt-4 text-lg text-slate-600 dark:text-slate-300">{post.summary}</p>
      )}
      <div className="mt-8">
        <ContentHtml html={post.content} />
      </div>
    </article>
  );
}
