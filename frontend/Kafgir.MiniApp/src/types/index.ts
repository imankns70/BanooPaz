export enum PaymentMethod { Cash = 1, CardToCard = 2, Online = 3 }
export enum DeliveryMethod { Pickup = 1, Delivery = 2 }
export enum OrderStatus { PendingConfirmation = 1, Confirmed = 2, Preparing = 3, Ready = 4, Delivered = 5, Cancelled = 6 }

export interface DailyMenuDto { id: number; menuDate: string; isOpen: boolean; note?: string | null; items: DailyMenuItemDto[] }
export interface DailyMenuItemDto {
  id: number; foodId: number; foodName: string; foodDescription?: string | null; imageUrl?: string | null
  price: number; capacityPortions: number; soldPortions: number; remainingPortions: number; isAvailable: boolean
}
export interface CreateOrderItemRequest { dailyMenuItemId: number; quantity: number }
export interface CustomerProfileLookupRequest {
  telegramInitData?: string | null
  telegramUserId?: number | null
  telegramUsername?: string | null
}
export interface CustomerAddressDto {
  id: number
  title: string
  city: string
  addressLine: string
  description?: string | null
  isDefault: boolean
}
export interface CustomerProfileDto {
  id: number
  userId: number
  preferredName: string
  defaultPhoneNumber: string
  addresses: CustomerAddressDto[]
}
export interface CreateOrderRequest {
  telegramInitData?: string | null
  telegramUserId?: number | null; telegramUsername?: string | null; fullName: string; phoneNumber: string
  customerAddressId?: number | null; newAddressTitle?: string | null; city: string; addressLine?: string | null
  addressDescription?: string | null; saveAddress?: boolean; paymentMethod: PaymentMethod
  deliveryMethod: DeliveryMethod; customerNote?: string | null; items: CreateOrderItemRequest[]
}
export interface OrderDto {
  id: number; orderNumber: string; status: OrderStatus; subtotalAmount: number; deliveryFee: number; totalAmount: number
}
export interface CartItem {
  dailyMenuItemId: number; foodName: string; unitPrice: number; quantity: number; remainingPortions: number
}
