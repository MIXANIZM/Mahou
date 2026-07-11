#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_bytes(relative_path: str, old: str, new: str, expected: int = 1) -> None:
    path = ROOT / relative_path
    data = path.read_bytes()
    old_bytes = old.encode("utf-8")
    new_bytes = new.encode("utf-8")
    count = data.count(old_bytes)
    if count != expected:
        raise RuntimeError(
            "%s: expected %d occurrence(s), found %d" % (relative_path, expected, count)
        )
    path.write_bytes(data.replace(old_bytes, new_bytes, expected))


replace_bytes(
    "Mahou/Classes/Auri.cs",
    "bool inb = false, inq = false, inqq = false, nextv = false, inbs = false, inbc = false, inbo = false, vals = false;",
    "bool inq = false, inqq = false, nextv = false, inbo = false, vals = false;",
)
replace_bytes(
    "Mahou/Classes/Auri.cs",
    "if (k == '{' && /*!inbs &&*/ !inq) { inb = true; z++; }",
    "if (k == '{' && !inq) { z++; }",
)
replace_bytes(
    "Mahou/Classes/Auri.cs",
    "if (k == '}' && /*!inbs &&*/ !inq) { inb = false; z--; }",
    "if (k == '}' && !inq) { z--; }",
)
replace_bytes(
    "Mahou/Classes/Auri.cs",
    "if (k == '[' && /*!inb &&*/ !inq) { inbs = true; q++; inbo = true; /*if (indblock) continue;*/ } else { inbo = false; }",
    "if (k == '[' && !inq) { q++; inbo = true; /*if (indblock) continue;*/ } else { inbo = false; }",
)
replace_bytes(
    "Mahou/Classes/Auri.cs",
    "if (k == ']' && /*!inb &&*/ !inq) { if (q == 1) { v=0; } inbs = false; q--; inbc = true; } else { inbc = false; }",
    "if (k == ']' && !inq) { if (q == 1) { v=0; } q--; }",
)
replace_bytes(
    "Mahou/Classes/KMHook.cs",
    "\t\t\t_selis, _mselis, snipselshiftpressed, snipselwassel, ",
    "\t\t\tsnipselshiftpressed, snipselwassel,",
)
replace_bytes(
    "Mahou/MahouUI.cs",
    "\t\tclass MTheme {\r\n"
    "\t\t\tpublic Color BG;\r\n"
    "\t\t\tpublic Color FG;\r\n"
    "\t\t\tpublic Color TAB_BORDERS;\r\n"
    "\t\t\tpublic Color TAB_FOCUS_BG;\r\n"
    "\t\t}\r\n",
    "",
)

Path(__file__).unlink()
print("Removed nine dead-code warnings without global suppression.")
