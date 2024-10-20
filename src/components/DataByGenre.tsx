import {
  _getCountrySales,
  _getGenreSales,
  _getTotalSales,
} from '@/actions/getActions';
import { COUNTRY_LIST, GENRE_LIST } from '@/consts';
import { CountrySalesData, GenreSalesData, SalesData } from '@/types/type';
import {
  Box,
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

export function DataByGenre() {
  const [genreData, setGenreData] = useState<GenreSalesData[]>([]);
  const [selectedMetric, setSelectedMetric] = useState<
    'totalSales' | 'numberOfSales'
  >('totalSales');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const [genreSelected, setGenreSelected] = useState<
    (typeof GENRE_LIST)[number]
  >(GENRE_LIST[1]);
  const [startDate, setStartDate] = useState<Dayjs | null>(null);
  const [endDate, setEndDate] = useState<Dayjs | null>(null);
  const [linearEstimate, setLinearEstimate] = useState<number | null>(null);
  const [monthYearInput, setMonthYearInput] = useState<string>('');

  useEffect(() => {
    getData(genreSelected, startDate, endDate);
  }, [genreSelected, startDate, endDate]);

  const getData = async (
    genre: string,
    start: Dayjs | null,
    end: Dayjs | null
  ) => {
    const startString = start ? start.toISOString().split('T')[0] : '';
    const endString = end ? end.toISOString().split('T')[0] : '';

    const data = await _getGenreSales(genre, startString, endString);
    setGenreData(data);
  };

  const handleMetricChange = (event: any) => {
    setSelectedMetric(event.target.value as 'totalSales' | 'numberOfSales');
  };

  const handleGenreChange = (event: any) => {
    setGenreSelected(event.target.value as (typeof GENRE_LIST)[number]);
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
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
    const x = genreData.map((row) => {
      const monthDate = new Date(row.month);
      return monthDate.getFullYear() * 12 + monthDate.getMonth();
    });

    const y = genreData.map((row) => row.totalSales);
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

  const data = genreData.map((data) => {
    const monthDate = new Date(data.month);
    const month = String(monthDate.getMonth() + 1).padStart(2, '0');
    const year = monthDate.getFullYear();
    return {
      month: `${month}-${year}`,
      genre: data.genre ?? 'All',
      totalSales: parseFloat(data.totalSales.toFixed(2)),
      numberOfSales: parseFloat(data.numberOfSales.toFixed(2)),
    };
  });

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const { month, genre } = payload[0].payload;
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
          <InputLabel>Genre</InputLabel>
          <Select value={genreSelected} onChange={handleGenreChange} fullWidth>
            {GENRE_LIST.slice(1).map((genre) => {
              return <MenuItem value={genre}>{genre}</MenuItem>;
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

        <Box width={'20%'}>
          <InputLabel>Start Day</InputLabel>
          <DatePicker
            value={startDate}
            onChange={(newValue: Dayjs | null) => {
              setStartDate(newValue);
            }}
            maxDate={endDate ?? dayjs()}
          />
        </Box>

        <Box width={'20%'}>
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
              <TableCell style={{ width: '33%' }}>Genre</TableCell>
              <TableCell style={{ width: '33%' }}>Month</TableCell>
              <TableCell style={{ width: '33%' }}>
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
                  <TableCell style={{ width: '33%' }}>
                    {row.genre ?? 'All'}
                  </TableCell>
                  <TableCell style={{ width: '33%' }}>{row.month}</TableCell>
                  <TableCell style={{ width: '33%' }}>
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
    </>
  );
}

export default DataByGenre;
