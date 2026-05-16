export interface ReviewDto {
  id: string;
  userId: string;
  userFullName: string;
  productId: string;
  rating: number;
  comment: string;
  createdAt: string;
  imageUrls: string[];
}

export interface AddReviewRequest {
  productId: string;
  rating: number;
  comment: string;
}
