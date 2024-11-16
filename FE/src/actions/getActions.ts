import axios from 'axios';
import { CountrySalesData, GenreSalesData, SalesData } from '@/types/type';
import { saveAs } from 'file-saver';

export const _getTotalSales = async (
  dateStart?: string,
  dateEnd?: string
): Promise<SalesData[]> => {
  let url = `http://localhost:5036/totalsales?`;

  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  try {
    const response = await axios.get<SalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};

export const _exportTotalSales = async (
  dateStart: string | undefined,
  dateEnd: string | undefined,
  monthIntoFuture: number,
  type: number
): Promise<void> => {
  let url = `http://localhost:5036/totalsales/export?`;

  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  url += `&forecastMonths=${monthIntoFuture}&trendFunction=${type}`;
  try {
    const response = await axios.get(url, {
      responseType: 'blob',
    });

    saveAs(new Blob([response.data]), 'TotalSales.xlsx');
    console.log('File successfully saved.');
  } catch (err) {
    console.error('Error exporting sales data:', err);
  }
};

export const _getGenreSales = async (
  genre: string,
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `http://localhost:5036/genresales/${genre}?`;

  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  try {
    const response = await axios.get<GenreSalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};

export const _exportGenreSales = async (
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `http://localhost:5036/genresales/export?`;

  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  try {
    const response = await axios.get(url, {
      responseType: 'blob',
    });

    saveAs(new Blob([response.data]), 'TotalSalesByGenre.xlsx');
    console.log('File successfully saved.');
  } catch (err) {
    console.error('Error exporting sales data:', err);
  }
};

export const _getCountrySales = async (
  country: string,
  genre: string,
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `http://localhost:5036`;
  if (genre != 'All') {
    url = `${url}/countryGenreSales/${country}/${genre}`;
  } else {
    url = `${url}/countrySales/${country}`;
  }
  url += '?';
  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  try {
    const response = await axios.get<CountrySalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};

export const _exportCountrySales = async (
  country: string,
  genre: string,
  dateStart: string | undefined,
  dateEnd: string | undefined,
  monthIntoFuture: number,
  type: number
) => {
  let url = `http://localhost:5036`;
  if (genre != 'All') {
    url = `${url}/countryGenreSales/export/${country}/${genre}`;
  } else {
    url = `${url}/countrySales/export/${country}`;
  }
  url += '?';
  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  url += `&forecastMonths=${monthIntoFuture}&trendFunction=${type}`;
  try {
    const response = await axios.get(url, {
      responseType: 'blob',
    });

    saveAs(new Blob([response.data]), 'TotalSales.xlsx');
    console.log('File successfully saved.');
  } catch (err) {
    console.error('Error exporting sales data:', err);
  }
};


export const _getCountryTotalSales = async (
  genre: string,
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `http://localhost:5036`;
  if (genre != 'All') {
    url = `${url}/countryGenreSalesAll/${genre}`;
  } else {
    url = `${url}/countrySalesAll`;
  }
  url += '?';
  if (dateStart) {
    url = `${url}startDate=${dateStart}`;
  }

  if (dateEnd) {
    url = `${url}&endDate=${dateEnd}`;
  }

  try {
    const response = await axios.get<CountrySalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};