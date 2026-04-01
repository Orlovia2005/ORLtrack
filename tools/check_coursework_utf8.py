import zipfile
from pathlib import Path
path = Path(r'C:\Users\ignat\source\repos\movieRecom\ORLtrack_coursework_Orlov.docx')
with zipfile.ZipFile(path) as z:
    xml = z.read('word/document.xml').decode('utf-8')
for sample in [
    'Листинг 2 — Фрагмент логики фиксации пропуска без списания баланса',
    'Листинг 3 — Фрагмент расчета уровня риска ученика',
    'Рисунок 11 — Дополнительные материалы по интерфейсу и архитектуре ORLtrack'
]:
    print(sample, sample in xml)
