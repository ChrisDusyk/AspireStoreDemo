export interface ProductResponse {
  id: string;
  name: string | null;
  description: string | null;
  price: number | null;
  isActive: boolean;
  createdDate: string;
  updatedDate: string;
}
