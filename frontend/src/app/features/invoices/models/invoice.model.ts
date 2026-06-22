export type InvoiceType = 'Sales' | 'Purchase' | 'CreditNote';
export type InvoiceStatus = 'Draft' | 'Issued' | 'Printed' | 'Signed' | 'Cancelled';

export interface InvoiceLine {
  id: string;
  inventoryItemId: string;
  inventoryItemName?: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  taxRate: number;
  taxAmount: number;
  notes?: string | null;
}

export interface Invoice {
  id: string;
  number: string;
  type: InvoiceType;
  status: InvoiceStatus;
  salesOrderId?: string | null;
  salesOrderNumber?: string | null;
  purchaseOrderId?: string | null;
  customerId: string;
  customerName?: string | null;
  supplierId?: string | null;
  supplierName?: string | null;
  warehouseId: string;
  warehouseName?: string | null;
  issuedAtUtc: string;
  printedAtUtc?: string | null;
  signedAtUtc?: string | null;
  dueDateUtc?: string | null;
  totalAmount: number;
  taxAmount: number;
  currency: string;
  exchangeRate: number;
  paymentType?: string | null;
  printedBy?: string | null;
  signedByName?: string | null;
  signatureDataUrl?: string | null;
  notes?: string | null;
  externalReference?: string | null;
  createdAt: string;
  lines: InvoiceLine[];
}

export interface InvoiceSummary {
  id: string;
  number: string;
  type: InvoiceType;
  status: InvoiceStatus;
  customerId: string;
  customerName?: string | null;
  warehouseId: string;
  warehouseName?: string | null;
  issuedAtUtc: string;
  createdAt: string;
  totalAmount: number;
  taxAmount: number;
  currency: string;
  salesOrderId?: string | null;
  salesOrderNumber?: string | null;
  lineCount: number;
}

export interface CreateInvoiceRequest {
  salesOrderId?: string | null;
  customerId: string;
  warehouseId: string;
  currency: string;
  exchangeRate: number;
  paymentType?: string | null;
  dueDateUtc?: string | null;
  notes?: string | null;
  lines: {
    inventoryItemId: string;
    quantity: number;
    unitPrice: number;
    taxRate?: number;
    notes?: string | null;
  }[];
}
