// actions/salesActions.js
import axios from 'axios';
import { CountrySalesData, GenreSalesData, SalesData } from '@/types/type';

export const _getTotalSales = async (
  dateStart?: string,
  dateEnd?: string
): Promise<SalesData[]> => {
  let url = `https://3dmb718n-5000.euw.devtunnels.ms/totalsales?`;

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

export const _getGenreSales = async (
  genre: string,
  dateStart?: string,
  dateEnd?: string
) => {
  const url = `https://3dmb718n-5000.euw.devtunnels.ms/genresales/${genre}?startDate=${dateStart}&endDate=${dateEnd}`;
  try {
    const response = await axios.get<GenreSalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};

export const _getCountrySales = async (
  country: string,
  genre?: string,
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `https://3dmb718n-5000.euw.devtunnels.ms/countrySales/${country}`;
  if (genre) {
    url = `${url}/${genre}`;
  }

  url = `${url}?startDate=${dateStart}&endDate=${dateEnd}`;

  try {
    const response = await axios.get<CountrySalesData[]>(url);
    return response.data;
  } catch (err) {
    console.log(err);
    return [];
  }
};
