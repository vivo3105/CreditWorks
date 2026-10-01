// Browser copy of the API's category rules (CategoryConfigurationValidator,
// WeightRange) so problems show while typing. The API remains the authority.
//
// Both limits are inclusive: a category covers weights from its minimum up to
// and including its maximum. Weight ranges may not clash, which includes
// sharing a limit (one's minimum equal to another's maximum). Gaps are allowed;
// a vehicle whose weight falls in a gap is uncategorised.

interface Range {
  minWeight: number;
  // Inclusive; null = no upper limit.
  maxWeight: number | null;
}

const numberFormat = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 });

export function formatWeight(kg: number): string {
  return `${numberFormat.format(kg)} kg`;
}

export function formatRange(min: number, max: number | null): string {
  return max === null
    ? `${formatWeight(min)} and above`
    : `${numberFormat.format(min)} – ${formatWeight(max)}`;
}

export function findCategoryForWeight<T extends Range>(categories: T[], weight: number): T | null {
  return (
    categories.find((c) => weight >= c.minWeight && (c.maxWeight === null || weight <= c.maxWeight)) ?? null
  );
}

// Same test as WeightRange.Overlaps: sharing a limit counts as a clash.
export function rangesClash(a: Range, b: Range): boolean {
  const aEndsAtOrAfterBStarts = a.maxWeight === null || a.maxWeight >= b.minWeight;
  const bEndsAtOrAfterAStarts = b.maxWeight === null || b.maxWeight >= a.minWeight;
  return aEndsAtOrAfterBStarts && bEndsAtOrAfterAStarts;
}

// The first category (lightest first) that `range` clashes with, ignoring the
// category with id `exceptId` (the one being edited). Null if none.
export function findClash<T extends Range & { id: number }>(
  categories: T[],
  range: Range,
  exceptId: number | null,
): T | null {
  return (
    [...categories]
      .sort((a, b) => a.minWeight - b.minWeight)
      .find((c) => c.id !== exceptId && rangesClash(c, range)) ?? null
  );
}

// Weights have 2 decimals, so the next weight after `kg` is kg + 0.01. Work in
// whole cents to avoid floating-point drift (0.1 + 0.2 !== 0.3).
const toCents = (kg: number) => Math.round(kg * 100);
const fromCents = (cents: number) => cents / 100;

// Weight ranges from 0 kg upwards that no category covers (limits inclusive).
export function findGaps(categories: Range[]): Range[] {
  const gaps: Range[] = [];
  let next = 0; // lowest weight (in cents) not yet known to be covered

  for (const c of [...categories].sort((a, b) => a.minWeight - b.minWeight)) {
    const min = toCents(c.minWeight);
    if (min > next) {
      gaps.push({ minWeight: fromCents(next), maxWeight: fromCents(min - 1) });
    }
    if (c.maxWeight === null) {
      return gaps; // open-ended: covers everything above
    }
    next = Math.max(next, toCents(c.maxWeight) + 1);
  }

  gaps.push({ minWeight: fromCents(next), maxWeight: null });
  return gaps;
}
