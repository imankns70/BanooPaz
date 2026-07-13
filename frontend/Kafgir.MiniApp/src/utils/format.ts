export const toPersianDigits = (value: string | number): string =>
  String(value).replace(/\d/g, (digit) => '۰۱۲۳۴۵۶۷۸۹'[Number(digit)])

export const formatMoney = (amount: number): string =>
  `${toPersianDigits(new Intl.NumberFormat('fa-IR').format(amount))} تومان`
