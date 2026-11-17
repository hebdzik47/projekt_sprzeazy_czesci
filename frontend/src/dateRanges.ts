export type DateRange = { from: string; to: string };
export const presets = [ ['month', 'Ten miesiąc'], ['previous', 'Poprzedni miesiąc'], ['week', 'Ten tydzień'], ['lastWeek', 'Zeszły tydzień'], ['today', 'Dzisiaj'] ] as const;
export function dateRange(kind: string, now = new Date()): DateRange {
  const start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const end = new Date(start);
  if (kind === 'month') { start.setDate(1); end.setMonth(end.getMonth() + 1, 0); }
  if (kind === 'previous') { start.setMonth(start.getMonth() - 1, 1); end.setDate(0); }
  if (kind === 'week' || kind === 'lastWeek') {
    start.setDate(start.getDate() - (start.getDay() + 6) % 7 - (kind === 'lastWeek' ? 7 : 0));
    end.setTime(start.getTime()); end.setDate(end.getDate() + 6);
  }
  const local = (d: Date) => [d.getFullYear(), String(d.getMonth()+1).padStart(2,'0'), String(d.getDate()).padStart(2,'0')].join('-');
  return { from: local(start), to: local(end) };
}
