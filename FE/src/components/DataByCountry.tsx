import {
  _exportCountrySales,
  _getCountrySales,
  _getCountryTotalSales,
} from '@/actions/getActions';
import { COUNTRY_LIST, GENRE_LIST, trendEstimationFunctions } from '@/consts';
import { CountrySalesData } from '@/types/type';
import {
  Box,
  Button,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Typography,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs, { Dayjs } from 'dayjs';
import { SimpleLinearRegression } from 'ml-regression-simple-linear';
import { useEffect, useState } from 'react';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { Map } from './Map/Map';

export function DataByCountry() {
  const [countryData, setCountryData] = useState<CountrySalesData[]>([]);
  const [countryAllData, setCountryAllData] = useState<CountrySalesData[]>([]);

  const [selectedMetric, setSelectedMetric] = useState<
    'totalSales' | 'numberOfSales'
  >('totalSales');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const [countrySelected, setCountrySelected] = useState<
    (typeof COUNTRY_LIST)[number]
  >(COUNTRY_LIST[0]);
  const [genreSelected, setGenreSelected] = useState<
    (typeof GENRE_LIST)[number]
  >(GENRE_LIST[0]);
  const [startDate, setStartDate] = useState<Dayjs | null>(null);
  const [endDate, setEndDate] = useState<Dayjs | null>(null);
  const [linearEstimate, setLinearEstimate] = useState<number | null>(null);
  const [monthYearInput, setMonthYearInput] = useState<string>('');
  const [trendingType, setTrendingType] = useState<number>(0);
  const [monthsIntoFuture, setMonthsIntoFuture] = useState<number>(0);
  useEffect(() => {
    getData(countrySelected, genreSelected, startDate, endDate);
  }, [countrySelected, genreSelected, startDate, endDate]);

  const getData = async (
    country: string,
    genre: string,
    start: Dayjs | null,
    end: Dayjs | null
  ) => {
    const startString = start ? start.toISOString().split('T')[0] : '';
    const endString = end ? end.toISOString().split('T')[0] : '';

    const data = await _getCountrySales(country, genre, startString, endString);
    const data2 = await _getCountryTotalSales(genre, startString, endString);

    setCountryAllData(data2);
    setCountryData(data);
  };

  const handleMetricChange = (event: any) => {
    setSelectedMetric(event.target.value as 'totalSales' | 'numberOfSales');
  };

  const handleCountryChange = (event: any) => {
    setCountrySelected(event.target.value as (typeof COUNTRY_LIST)[number]);
  };

  const handleGenreChange = (event: any) => {
    setGenreSelected(event.target.value as (typeof GENRE_LIST)[number]);
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleTrendingChange = (event: any) => {
    setTrendingType(event.target.value);
  };

  const handleChangeRowsPerPage = (
    event: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>
  ) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleEstimate = () => {
    const monthYearPattern = /^(0[1-9]|1[0-2])-\d{4}$/; // Regular expression for MM-YYYY format

    if (!monthYearInput.match(monthYearPattern)) {
      alert('Please enter the date in the format MM-YYYY');
      return; // Exit the function if format is invalid
    }
    const x = countryData.map((row) => {
      const monthDate = new Date(row.month);
      return monthDate.getFullYear() * 12 + monthDate.getMonth();
    });

    const y = countryData.map((row) => row.totalSales);
    const regression = new SimpleLinearRegression(x, y);

    if (monthYearInput) {
      const [month, year] = monthYearInput.split('-').map(Number);
      if (year < 2000) {
        alert('Please enter a year after 2000');
        return;
      }
      const monthNumber = year * 12 + (month - 1);
      const predicted = regression.predict(monthNumber);
      setLinearEstimate(predicted);
    }
  };

  const data = countryData.map((data) => {
    const monthDate = new Date(data.month);
    const month = String(monthDate.getMonth() + 1).padStart(2, '0');
    const year = monthDate.getFullYear();
    return {
      customerCountry: data.customerCountry,
      month: `${month}-${year}`,
      genre: data.genre ?? 'All',
      totalSales: parseFloat(data.totalSales.toFixed(2)),
      numberOfSales: parseFloat(data.numberOfSales.toFixed(2)),
    };
  });

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const { month, customerCountry, genre } = payload[0].payload;
      return (
        <Paper
          elevation={3}
          sx={{
            padding: 2,
            backgroundColor: 'white',
            border: '1px solid #ccc',
          }}
        >
          <Box>
            <Typography variant="subtitle1" color="textPrimary">
              {`Month: ${month}`}
            </Typography>
            <Typography variant="subtitle2" color="textSecondary">
              {`Country: ${customerCountry}`}
            </Typography>
            <Typography variant="subtitle2" color="textSecondary">
              {`Genre: ${genre}`}
            </Typography>
            <Typography variant="body2" color="textPrimary">
              {`Sales: $${payload[0].value}`}
            </Typography>
          </Box>
        </Paper>
      );
    }
    return null;
  };

  return (
    <>
      <Box display="flex" alignItems={'center'} gap={2} marginBottom={2}>
        <Box width={'20%'}>
          <InputLabel>Country</InputLabel>
          <Select
            value={countrySelected}
            onChange={handleCountryChange}
            fullWidth
          >
            {COUNTRY_LIST.map((country) => {
              return (
                <MenuItem value={country} key={country}>
                  {country}
                </MenuItem>
              );
            })}
          </Select>
        </Box>

        <Box width={'20%'}>
          <InputLabel>Genre</InputLabel>
          <Select value={genreSelected} onChange={handleGenreChange} fullWidth>
            {GENRE_LIST.map((genre) => {
              return (
                <MenuItem value={genre} key={genre}>
                  {genre}
                </MenuItem>
              );
            })}
          </Select>
        </Box>

        <Box width={'20%'}>
          <InputLabel>Type</InputLabel>
          <Select
            value={selectedMetric}
            onChange={handleMetricChange}
            fullWidth
          >
            <MenuItem value="totalSales">Total Sales</MenuItem>
            <MenuItem value="numberOfSales">Number of Sales</MenuItem>
          </Select>
        </Box>

        <Box>
          <InputLabel>Start Day</InputLabel>
          <DatePicker
            value={startDate}
            onChange={(newValue: Dayjs | null) => {
              setStartDate(newValue);
            }}
            maxDate={endDate ?? dayjs()}
          />
        </Box>

        <Box>
          <InputLabel>End Day</InputLabel>
          <DatePicker
            value={endDate}
            onChange={(newValue: Dayjs | null) => {
              setEndDate(newValue);
            }}
            minDate={startDate ?? undefined}
            maxDate={dayjs()}
          />
        </Box>
      </Box>
      <Box display="flex" alignItems={'center'} gap={2} marginBottom={2}>
        <InputLabel>Estimate Total Sales for Month-Year</InputLabel>
        <input
          type="text"
          placeholder="MM-YYYY"
          value={monthYearInput}
          onChange={(e) => setMonthYearInput(e.target.value)}
        />
        <button onClick={handleEstimate}>Estimate</button>
        {linearEstimate !== null && (
          <div>
            <h4>Estimated Total Sales: ${linearEstimate.toFixed(2)}</h4>
          </div>
        )}
      </Box>
      <Box
        display="flex"
        justifyContent={'center'}
        gap={2}
        marginBottom={2}
        flexDirection={'column'}
        width={'100%'}
      >
        <InputLabel>Export section</InputLabel>
        <Box width={'25%'}>
          <InputLabel>Function Type</InputLabel>
          <Select value={trendingType} onChange={handleTrendingChange}>
            {trendEstimationFunctions.map((func, index) => {
              return (
                <MenuItem value={index} key={index}>
                  {func}
                </MenuItem>
              );
            })}
          </Select>
        </Box>
        <Box display="flex" alignItems={'center'} gap={2} marginBottom={2}>
          <InputLabel>Months into the future</InputLabel>
          <input
            type="number"
            value={monthsIntoFuture}
            onChange={(e) => {
              const value = parseInt(e.target.value);
              if (value >= 0) {
                setMonthsIntoFuture(value);
              }
            }}
          />
        </Box>
        <Box>
          <Button
            variant="contained"
            onClick={() => {
              const startString = startDate
                ? startDate.toISOString().split('T')[0]
                : '';
              const endString = endDate
                ? endDate.toISOString().split('T')[0]
                : '';
              _exportCountrySales(
                countrySelected,
                genreSelected,
                startString,
                endString,
                monthsIntoFuture,
                trendingType
              );
            }}
          >
            Export
          </Button>
        </Box>
      </Box>
      <ResponsiveContainer width="100%" height={250}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="month" />
          <YAxis />
          <Tooltip content={<CustomTooltip />} />
          <Legend />
          <Line
            type="monotone"
            dataKey={selectedMetric}
            stroke={selectedMetric === 'totalSales' ? '#8884d8' : '#82ca9d'}
            strokeWidth={2}
          />
        </LineChart>
      </ResponsiveContainer>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: '25%' }}>Country</TableCell>
              <TableCell style={{ width: '25%' }}>Genre</TableCell>
              <TableCell style={{ width: '25%' }}>Month</TableCell>
              <TableCell style={{ width: '25%' }}>
                {selectedMetric === 'totalSales'
                  ? 'Total Sales'
                  : 'Number of Sales'}
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data
              .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
              .map((row, index) => (
                <TableRow key={index}>
                  <TableCell style={{ width: '25%' }}>
                    {row.customerCountry}
                  </TableCell>
                  <TableCell style={{ width: '25%' }}>
                    {row.genre ?? 'All'}
                  </TableCell>
                  <TableCell style={{ width: '25%' }}>{row.month}</TableCell>
                  <TableCell style={{ width: '25%' }}>
                    {selectedMetric === 'totalSales'
                      ? `$${row.totalSales}`
                      : row.numberOfSales}
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        rowsPerPageOptions={[5, 10, 25]}
        component="div"
        count={data.length}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />
      <Box display="flex" justifyContent={'center'} alignItems={'center'}>
        <Map countryData={countryAllData} />
      </Box>
    </>
  );
}

export default DataByCountry;
