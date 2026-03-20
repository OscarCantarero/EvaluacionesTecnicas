export interface ResultadoPaginado<T> {
  readonly items: ReadonlyArray<T>;
  readonly totalItems: number;
  readonly pagina: number;
  readonly tamanoPagina: number;
  readonly totalPaginas: number;
  readonly tieneSiguiente: boolean;
  readonly tieneAnterior: boolean;
}
