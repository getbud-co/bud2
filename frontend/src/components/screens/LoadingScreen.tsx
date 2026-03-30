export function LoadingScreen() {
  return (
    <div className="flex h-dvh w-full items-center justify-center bg-[var(--color-caramel-50)]">
      <div className="w-9 h-9 animate-spin rounded-full border-2 border-[var(--color-caramel-200)] border-t-[var(--color-neutral-900)]" />
    </div>
  );
}
