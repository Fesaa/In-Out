import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {Product, ProductCategory} from '../_models/product';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly baseUrl = environment.apiUrl + 'products/';
  private readonly http = inject(HttpClient);

  allProducts(onlyEnabled: boolean = false): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}?onlyEnabled=${onlyEnabled}`);
  }

  getCategories(onlyEnabled: boolean = false): Observable<ProductCategory[]> {
    return this.http.get<ProductCategory[]>(`${this.baseUrl}category?onlyEnabled=${onlyEnabled}`);
  }

  getProductsByCategory(category: ProductCategory): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}category/${category}`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}${id}`);
  }

  getByIds(ids: number[]) {
    return this.http.post<Product[]>(`${this.baseUrl}by-id`, ids)
  }

  createProduct(product: Product): Observable<void> {
    return this.http.post<void>(this.baseUrl, product);
  }

  updateProduct(product: Product): Observable<void> {
    return this.http.put<void>(this.baseUrl, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}${id}`);
  }

  createCategory(category: ProductCategory): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}category`, category);
  }

  updateCategory(category: ProductCategory): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}category`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}category/${id}`);
  }

  orderCategories(ids: number[]) {
    return this.http.post(`${this.baseUrl}category/order`, ids);
  }
}
