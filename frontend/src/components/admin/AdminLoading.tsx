export function AdminLoading({ label = "Yükleniyor..." }: { label?: string }) {
  return (
    <div className="flex min-h-[40vh] flex-col items-center justify-center gap-4">
      <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/10 border-t-blue-500" />
      <p className="text-sm text-slate-400">{label}</p>
    </div>
  );
}
