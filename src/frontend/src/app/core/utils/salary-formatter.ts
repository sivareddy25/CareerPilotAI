/**
 * Converts any raw user input (numbers, '140k', '12k/mo', '$75/hr', '140000 per year')
 * into a normalized annual salary string format: '$140,000 / year'.
 */
export function formatSalaryToAnnual(rawInput: string): string {
  if (!rawInput || !rawInput.trim()) {
    return '$140,000 / year';
  }

  const text = rawInput.trim().toLowerCase();

  // Extract digits and optional multiplier/period hints
  const isMonthly = text.includes('mo') || text.includes('month');
  const isHourly = text.includes('hr') || text.includes('hour');

  // Parse numeric component
  let numMatch = text.match(/[\d,.]+/);
  if (!numMatch) {
    return '$140,000 / year';
  }

  let numStr = numMatch[0].replace(/,/g, '');
  let val = parseFloat(numStr);

  if (isNaN(val) || val <= 0) {
    return '$140,000 / year';
  }

  // Handle 'k' multiplier (e.g. 140k -> 140,000)
  if (text.includes('k') && val < 1000) {
    val *= 1000;
  } else if (text.includes('m') && val < 1000) {
    val *= 1000000;
  }

  // Convert monthly or hourly to annual
  if (isMonthly) {
    val *= 12;
  } else if (isHourly) {
    val *= 2080; // Standard 40 hrs/wk * 52 wks
  }

  // Format to integer currency
  const formattedAmount = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(Math.round(val));

  return `${formattedAmount} / year`;
}
