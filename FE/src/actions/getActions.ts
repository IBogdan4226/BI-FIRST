import axios from 'axios';
import { CountrySalesData, GenreSalesData, SalesData } from '@/types/type';

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

export const _getCountrySales = async (
  country: string,
  genre: string,
  dateStart?: string,
  dateEnd?: string
) => {
  let url = `http://localhost:5036`;
  if (genre != 'All') {
    url = `${url}/countryGenreSales/${country}/${genre}`;
  }else{
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
