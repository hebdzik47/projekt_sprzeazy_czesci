import { jsPDF } from 'jspdf';
import { autoTable } from 'jspdf-autotable';
import type { Summary, Group } from './services';

type Range = { from: string; to: string };
const date = (value: string) => value.split('-').reverse().join('.');
const currency = (value: number) => value.toLocaleString('pl-PL', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace(/\u00a0/g, ' ') + ' zł';

// Kept separate from the browser download so the exact document can be verified in tests.
export function createReportPdf(data: Summary, range: Range, font: string) {
  const doc = new jsPDF({ compress: true, putOnlyUsedFonts: true });
  doc.addFileToVFS('NotoSans.ttf', font);
  doc.addFont('NotoSans.ttf', 'NotoSans', 'normal');
  doc.setFont('NotoSans');
  doc.setProperties({ title: `Raport sprzedaży ${range.from} - ${range.to}`, author: 'Megamot', subject: 'Raport sprzedaży netto' });
  doc.setFillColor(16, 44, 74); doc.rect(0, 0, 210, 47, 'F');
  doc.setTextColor(218, 236, 252); doc.setFontSize(11); doc.text('MEGAMOT / RAPORT FIRMOWY', 16, 15);
  doc.setTextColor(255); doc.setFontSize(22); doc.text('Raport sprzedaży', 16, 28);
  doc.setFontSize(10); doc.text(`Okres: ${date(range.from)} - ${date(range.to)} (włącznie)`, 16, 39);
  doc.setTextColor(24, 51, 79); doc.setFontSize(10);
  doc.text(`Wartość sprzedaży netto: ${currency(data.totalValue)}`, 16, 60);
  doc.text(`Sprzedane części (szt.): ${data.partsCount}`, 16, 68);
  doc.setFontSize(8); doc.setTextColor(100); doc.text('Wartości uwzględniają ilość sztuk. Wszystkie kwoty są kwotami netto w PLN.', 16, 78);
  let y = 90;
  function table(title: string, rows: Group[]) {
    if (y > 240) { doc.addPage(); y = 22; }
    doc.setTextColor(23, 76, 121); doc.setFontSize(12); doc.text(title, 16, y);
    autoTable(doc, {
      startY: y + 5,
      head: [['Pozycja', 'Liczba części', 'Wartość netto']],
      body: rows.length ? rows.map(r => [r.name, String(r.count), currency(r.value)]) : [['Brak sprzedaży w tym okresie', '0', currency(0)]],
      styles: { font: 'NotoSans', fontStyle: 'normal', fontSize: 9, cellPadding: 3, overflow: 'linebreak' },
      headStyles: { fontStyle: 'normal', fillColor: [23, 105, 170] },
      alternateRowStyles: { fillColor: [240, 246, 252] },
      columnStyles: { 0: { cellWidth: 100 }, 1: { cellWidth: 30, halign: 'right' }, 2: { cellWidth: 48, halign: 'right' } },
      margin: { left: 16, right: 16, top: 22, bottom: 23 },
      rowPageBreak: 'avoid',
      didDrawPage: (hook) => { y = (hook.cursor?.y ?? 22) + 15; },
    });
  }
  table('Sprzedaż według sprzedawców', data.byUser);
  table('Sprzedaż według samochodów', data.byCar);
  const count = doc.getNumberOfPages();
  const generated = new Date().toLocaleString('pl-PL');
  for (let page = 1; page <= count; page++) {
    doc.setPage(page); doc.setFont('NotoSans'); doc.setFontSize(8); doc.setTextColor(105);
    if (page > 1) doc.text(`Megamot | Raport sprzedaży | ${date(range.from)} - ${date(range.to)}`, 16, 12);
    doc.setDrawColor(220); doc.line(16, 278, 194, 278);
    doc.text(`Wygenerowano: ${generated}`, 16, 284);
    doc.text(`Strona ${page} / ${count}`, 194, 284, { align: 'right' });
  }
  return doc;
}

export async function downloadReportPdf(data: Summary, range: Range) {
  const response = await fetch('/fonts/NotoSans-Regular.ttf');
  if (!response.ok) throw new Error('Nie można pobrać czcionki raportu. Odśwież stronę i spróbuj ponownie.');
  const bytes = new Uint8Array(await response.arrayBuffer());
  let binary = '';
  for (let i = 0; i < bytes.length; i += 8192) binary += String.fromCharCode(...bytes.subarray(i, i + 8192));
  const filename = `raport-sprzedazy_${range.from}_${range.to}.pdf`;
  const url = URL.createObjectURL(createReportPdf(data, range, btoa(binary)).output('blob'));
  const link = document.createElement('a');
  link.href = url; link.download = filename;
  document.body.appendChild(link); link.click(); link.remove();
  return { url, filename };
}
