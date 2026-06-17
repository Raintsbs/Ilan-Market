export function ContentHtml({ html }: { html: string }) {
  return (
    <div
      className="prose prose-slate max-w-none dark:prose-invert prose-headings:text-slate-900 dark:prose-headings:text-white prose-a:text-blue-600"
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}
