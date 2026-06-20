export interface DeliveryRoute {
  id: string;
  code: string;
  date: string;
  vehicleId?: string;
  vehicleCode?: string;
  driverEmployeeId?: string;
  driverName?: string;
  status: string;
  notes?: string;
  stopCount: number;
}

export interface DeliveryRouteDetail extends DeliveryRoute {
  stops: DeliveryStop[];
}

export interface DeliveryStop {
  id: string;
  routeId: string;
  sequenceNumber: number;
  customerId?: string;
  customerName?: string;
  address: string;
  latitude?: number;
  longitude?: number;
  plannedArrivalUtc?: string;
  plannedDepartureUtc?: string;
  actualArrivalUtc?: string;
  actualDepartureUtc?: string;
  status: string;
  hasRegulatedGoods: boolean;
  notes?: string;
  shipmentCount: number;
  shipments: DeliveryShipment[];
}

export interface DeliveryStopDetail extends DeliveryStop {
  // shipments inherited from DeliveryStop
}

export interface DeliveryShipment {
  id: string;
  deliveryStopId: string;
  salesOrderId: string;
  salesOrderNumber?: string;
  customerName?: string;
  status: string;
}

export interface GeoPoint {
  latitude: number;
  longitude: number;
}

export interface Geofence {
  id: string;
  code: string;
  name: string;
  type: string;
  vertices: GeoPoint[];
  isActive: boolean;
}

export interface GeofenceCheckResult {
  geofenceId: string;
  code: string;
  name: string;
  type: string;
  isInside: boolean;
}

// ─── Request types ──────────────────────────────────────────

export interface CreateRouteRequest {
  code: string;
  date: string;
  vehicleId?: string;
  driverEmployeeId?: string;
  notes?: string;
}

export interface UpdateRouteRequest {
  code: string;
  date: string;
  vehicleId?: string;
  driverEmployeeId?: string;
  notes?: string;
}

export interface CreateStopRequest {
  routeId: string;
  sequenceNumber: number;
  customerId?: string;
  address: string;
  latitude?: number;
  longitude?: number;
  plannedArrivalUtc?: string;
  plannedDepartureUtc?: string;
  notes?: string;
}

export interface UpdateStopRequest {
  sequenceNumber: number;
  customerId?: string;
  address: string;
  latitude?: number;
  longitude?: number;
  plannedArrivalUtc?: string;
  plannedDepartureUtc?: string;
  notes?: string;
}

export interface AssignShipmentRequest {
  salesOrderId: string;
}

export interface UnassignedSalesOrder {
  id: string;
  number: string;
  customerName?: string;
  totalAmount: number;
  lineCount: number;
}

export interface CreateGeofenceRequest {
  code: string;
  name: string;
  type: string;
  vertices: GeoPoint[];
  isActive: boolean;
}

export interface UpdateGeofenceRequest {
  code: string;
  name: string;
  type: string;
  vertices: GeoPoint[];
  isActive: boolean;
}
