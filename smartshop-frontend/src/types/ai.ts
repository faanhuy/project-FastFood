export interface SemanticSearchResultDto {
  id: string;
  name: string;
  description: string;
  price: number;
  originalPrice: number;
  effectivePrice: number | null;
  slug: string;
  imageUrl: string | null;
  categoryId: string;
  score: number;
}

export interface SemanticSearchRequest {
  query: string;
  topN?: number;
  storeId?: string;
}

export interface GenerateDescriptionRequest {
  productName: string;
  categoryName: string;
}
