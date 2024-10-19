export interface SalesData {
    month: Date;
    totalSales: number;
    numberOfSales: number;
}

export interface GenreSalesData {
    genre: string;
    month: Date;
    totalSales: number;
    numberOfSales: number;
}

export interface CountrySalesData {
    month: Date;
    customerCountry: string;
    totalSales: number;
    numberOfSales: number;
    genre?: string;
}