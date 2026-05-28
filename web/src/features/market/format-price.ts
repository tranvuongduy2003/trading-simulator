export function formatPrice(price: number): string {
  const formatted = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 4,
  }).format(price)

  return formatted.replace(/(\.\d*?[1-9])0+$/u, '$1').replace(/\.0+$/u, '')
}
