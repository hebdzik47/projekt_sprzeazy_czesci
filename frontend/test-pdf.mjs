import { readFileSync, mkdirSync, writeFileSync } from 'node:fs';
import { createReportPdf } from './src/reportPdf.ts';
const config=JSON.parse(readFileSync('../backend/AutoPartsSales.Api/appsettings.Development.json','utf8').replace(/^\uFEFF/,''));
const login=await fetch('http://127.0.0.1:5180/api/auth/login',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({userName:config.InitialAdmin.UserName,password:config.InitialAdmin.Password})});
if(!login.ok)throw new Error('Login failed');
const {token}=await login.json();
const range={from:'2026-09-01',to:'2026-09-30'};
const response=await fetch('http://127.0.0.1:5180/api/reports/summary?'+new URLSearchParams(range),{headers:{Authorization:`Bearer ${token}`}});
if(!response.ok)throw new Error('Report failed');
const summary=await response.json();
const font=readFileSync('public/fonts/NotoSans-Regular.ttf').toString('base64');
mkdirSync('../tmp/pdfs',{recursive:true});
for(const [name,data] of [
 ['report',summary],
 ['empty',{totalValue:0,salesCount:0,partsCount:0,byUser:[],byCar:[]}],
 ['long',{...summary,byCar:Array.from({length:110},(_,i)=>({name:`Część ${i+1} - prawe lusterko, długa nazwa części z polskimi znakami: ą ć ę ł ń ó ś ź ż`,count:2,value:1234.56}))}]
]){
 const pdf=createReportPdf(data,range,font);
 writeFileSync(`../tmp/pdfs/${name}.pdf`,Buffer.from(pdf.output('arraybuffer')));
 console.log(`${name}: ${pdf.getNumberOfPages()} pages`);
}
writeFileSync('../tmp/pdfs/expected.json',JSON.stringify(summary));
