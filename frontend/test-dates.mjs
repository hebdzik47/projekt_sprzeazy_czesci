import assert from 'node:assert/strict';
import { dateRange } from './src/dateRanges.ts';
const checks = [
 ['month',new Date(2026,8,23), '2026-09-01','2026-09-30'],
 ['previous',new Date(2026,0,1), '2025-12-01','2025-12-31'],
 ['week',new Date(2026,8,23), '2026-09-21','2026-09-27'],
 ['week',new Date(2026,8,27), '2026-09-21','2026-09-27'],
 ['lastWeek',new Date(2026,8,21), '2026-09-14','2026-09-20'],
 ['today',new Date(2026,8,23), '2026-09-23','2026-09-23'],
 ['previous',new Date(2024,2,5), '2024-02-01','2024-02-29'],
];
for (const [key, now, from, to] of checks) assert.deepEqual(dateRange(key, now), {from,to});
console.log('OK: 7 testów zakresów dat, granica roku, niedziela i rok przestępny.');
