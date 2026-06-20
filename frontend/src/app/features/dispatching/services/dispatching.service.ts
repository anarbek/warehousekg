import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import {
  DeliveryRoute, DeliveryRouteDetail, DeliveryStop, DeliveryStopDetail,
  DeliveryShipment, Geofence, GeofenceCheckResult,
  CreateRouteRequest, UpdateRouteRequest,
  CreateStopRequest, UpdateStopRequest,
  AssignShipmentRequest,
  UnassignedSalesOrder,
  CreateGeofenceRequest, UpdateGeofenceRequest,
} from '../models/dispatching.model';

@Injectable({ providedIn: 'root' })
export class DispatchingService {
  private readonly http = inject(HttpClient);
  private readonly base = AppSettings.apiBaseUrl;

  // ─── Routes ────────────────────────────────────────────────────
  getRoutes(): Observable<DeliveryRoute[]> {
    return this.http.get<DeliveryRoute[]>(`${this.base}/routes`);
  }
  getRouteById(id: string): Observable<DeliveryRoute> {
    return this.http.get<DeliveryRoute>(`${this.base}/routes/${id}`);
  }
  getRouteDetail(id: string): Observable<DeliveryRouteDetail> {
    return this.http.get<DeliveryRouteDetail>(`${this.base}/routes/${id}/detail`);
  }
  createRoute(r: CreateRouteRequest): Observable<string> {
    return this.http.post<string>(`${this.base}/routes`, r);
  }
  updateRoute(id: string, r: UpdateRouteRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/routes/${id}`, r);
  }
  deleteRoute(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/routes/${id}`);
  }

  startRoute(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${id}/start`, null);
  }
  completeRoute(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${id}/complete`, null);
  }
  cancelRoute(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${id}/cancel`, null);
  }

  // ─── Stops ─────────────────────────────────────────────────────
  getStopsByRoute(routeId: string): Observable<DeliveryStop[]> {
    return this.http.get<DeliveryStop[]>(`${this.base}/routes/${routeId}/stops`);
  }
  getStopById(id: string): Observable<DeliveryStopDetail> {
    return this.http.get<DeliveryStopDetail>(`${this.base}/routes/_/stops/${id}`);
  }
  createStop(routeId: string, r: CreateStopRequest): Observable<string> {
    return this.http.post<string>(`${this.base}/routes/${routeId}/stops`, r);
  }
  updateStop(routeId: string, id: string, r: UpdateStopRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/routes/${routeId}/stops/${id}`, r);
  }
  deleteStop(routeId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/routes/${routeId}/stops/${id}`);
  }

  arriveAtStop(routeId: string, id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${routeId}/stops/${id}/arrive`, null);
  }
  completeStop(routeId: string, id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${routeId}/stops/${id}/complete`, null);
  }
  skipStop(routeId: string, id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/${routeId}/stops/${id}/skip`, null);
  }

  // ─── Sales Orders for dispatch ──────────────────────────────
  getUnassignedSalesOrders(): Observable<UnassignedSalesOrder[]> {
    return this.http.get<UnassignedSalesOrder[]>(`${this.base}/sales-orders/unassigned`);
  }

  // ─── Shipments ─────────────────────────────────────────────────
  getShipmentsByStop(stopId: string): Observable<DeliveryShipment[]> {
    return this.http.get<DeliveryShipment[]>(`${this.base}/routes/_/stops/${stopId}/shipments`);
  }
  assignShipment(stopId: string, r: AssignShipmentRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/routes/_/stops/${stopId}/shipments`, r);
  }
  removeShipment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/routes/_/stops/_/shipments/${id}`);
  }

  // ─── Geofences ─────────────────────────────────────────────────
  getGeofences(): Observable<Geofence[]> {
    return this.http.get<Geofence[]>(`${this.base}/geofences`);
  }
  getGeofenceById(id: string): Observable<Geofence> {
    return this.http.get<Geofence>(`${this.base}/geofences/${id}`);
  }
  checkStopAgainstGeofences(stopId: string): Observable<GeofenceCheckResult[]> {
    return this.http.get<GeofenceCheckResult[]>(`${this.base}/geofences/check/${stopId}`);
  }
  createGeofence(r: CreateGeofenceRequest): Observable<string> {
    return this.http.post<string>(`${this.base}/geofences`, r);
  }
  updateGeofence(id: string, r: UpdateGeofenceRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/geofences/${id}`, r);
  }
  deleteGeofence(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/geofences/${id}`);
  }
}
