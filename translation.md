# Translations (Cotton Library 0.3.0+)
## How to make your text compatible with more languages!

---
Here is an example of how you would make translations:
```
KEYS,en,ru
label.red_slime,"Red Slime","Красный слайм"
label.green_slime,"Green Slime","Зелёный слайм"
label.blue_slime,"Blue Slime","Синий слайм"
label.yellow_slime,"Yellow Slime","Жёлтый слайм"
label.cyan_slime,"Cyan Slime","Голубой слайм"
label.magenta_slime,"Magenta Slime","Пурпурный слайм"
label.chromatic_slime,"Chromatic Slime","Хроматический слайм"
label.red_plort,"Red Plort","Красный плорт"
label.green_plort,"Green Plort","Зелёный плорт"
label.blue_plort,"Blue Plort","Синий плорт"
label.yellow_plort,"Yellow Plort","Жёлтый плорт"
label.cyan_plort,"Cyan Plort","Голубой плорт"
label.magenta_plort,"Magenta Plort","Пурпурный плорт"
label.chromatic_plort,"Chromatic Plort","Хроматический плорт"
```
(This is from a mod called Color Slimes)

Here's how it works: 
1. Create a `.csv` file. You can name it anything! this example is named `localized.csv`
2. For the first line, start with defining your columns. The first column should be `KEYS`, and the other ones should be the languages. For example, `en` for English, `de` for German, `ru` for Russian.
3. All the other lines should be filling in the columns. Think of it as a spreadsheet, (because it literally is a file format for Excel!) where (in this example) it is a 3x15 table.
4. Right click on the file in your code editor and click `Properties`
5. Change the Build Action to `Embedded Resource`
6. Change all instances of `AddTranslation` in your code to use this new system.