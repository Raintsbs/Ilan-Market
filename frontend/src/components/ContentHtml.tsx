export function ContentHtml({ html }: { html: string }) {
  return (
    <div
      className="prose prose-slate max-w-none overflow-x-auto break-words dark:prose-invert prose-headings:text-slate-900 prose-img:max-w-full prose-table:block prose-table:overflow-x-auto dark:prose-headings:text-white prose-a:text-blue-600"
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}
