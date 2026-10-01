import { findCategoryForWeight, findClash, findGaps, formatRange, rangesClash } from './category-rules';

// Limits are inclusive.
const light = { id: 1, name: 'Light', minWeight: 0, maxWeight: 499.99 };
const medium = { id: 2, name: 'Medium', minWeight: 500, maxWeight: 2499.99 };
const heavy = { id: 3, name: 'Heavy', minWeight: 2500, maxWeight: null };
const all = [heavy, light, medium]; // unsorted on purpose

describe('findCategoryForWeight', () => {
  it('includes both the minimum and the maximum', () => {
    expect(findCategoryForWeight(all, 0)?.name).toBe('Light');
    expect(findCategoryForWeight(all, 499.99)?.name).toBe('Light');
    expect(findCategoryForWeight(all, 500)?.name).toBe('Medium');
    expect(findCategoryForWeight(all, 2499.99)?.name).toBe('Medium');
    expect(findCategoryForWeight(all, 2500)?.name).toBe('Heavy');
  });

  it('returns null for a weight in a gap', () => {
    expect(findCategoryForWeight([light, heavy], 1000)).toBeNull();
  });
});

describe('rangesClash / findClash', () => {
  it('allows neighbours one cent apart', () => {
    expect(rangesClash(light, medium)).toBe(false);
    expect(rangesClash(medium, heavy)).toBe(false);
  });

  it("treats a minimum equal to another's maximum as a clash", () => {
    expect(rangesClash(light, { minWeight: 499.99, maxWeight: 1000 })).toBe(true);
    expect(rangesClash({ minWeight: 0, maxWeight: 500 }, medium)).toBe(true);
  });

  it('finds overlaps, containment and open-ended clashes', () => {
    expect(findClash(all, { minWeight: 400, maxWeight: 600 }, null)?.name).toBe('Light');
    expect(findClash(all, { minWeight: 1000, maxWeight: 2000 }, null)?.name).toBe('Medium');
    expect(findClash(all, { minWeight: 5000, maxWeight: null }, null)?.name).toBe('Heavy');
  });

  it('ignores the category being edited', () => {
    expect(findClash(all, { minWeight: 600, maxWeight: 2400 }, 2)).toBeNull();
    expect(findClash(all, { minWeight: 499.99, maxWeight: 2499.99 }, 2)?.name).toBe('Light');
  });

  it('allows a range that fits a gap exactly', () => {
    expect(findClash([light, heavy], { minWeight: 500, maxWeight: 2499.99 }, null)).toBeNull();
  });
});

describe('findGaps', () => {
  const fmt = (gaps: { minWeight: number; maxWeight: number | null }[]) =>
    gaps.map((g) => formatRange(g.minWeight, g.maxWeight));

  it('finds no gaps when neighbours are one cent apart from 0 up', () => {
    expect(findGaps(all)).toEqual([]);
  });

  it('finds gaps at the start, in the middle and at the top, limits inclusive', () => {
    expect(fmt(findGaps([{ minWeight: 100, maxWeight: 499.99 }, { minWeight: 2500, maxWeight: 4999.99 }])))
      .toEqual(['0 – 99.99 kg', '500 – 2,499.99 kg', '5,000 kg and above']);
  });

  it('treats no categories as one big gap', () => {
    expect(fmt(findGaps([]))).toEqual(['0 kg and above']);
  });
});
