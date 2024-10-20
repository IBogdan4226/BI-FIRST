import DataByTotalSales from '@/components/DataByTotalSales';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider/LocalizationProvider';
import { Box, Button } from '@mui/material';
import { useState } from 'react';
import DataByCountry from '@/components/DataByCountry';
import DataByGenre from '@/components/DataByGenre';

enum SelectedView {
  TotalSales = 'TOTAL_SALES',
  ByGenre = 'BY_GENRE',
  ByCountry = 'BY_COUNTRY',
}

export function App() {
  const [selectedView, setSelectedView] = useState<SelectedView>(
    SelectedView.TotalSales
  );

  const handleViewChange = (view: SelectedView) => {
    setSelectedView(view);
  };

  return (
    <Box>
      <LocalizationProvider dateAdapter={AdapterDayjs}>
        {/* View Header with Buttons */}
        <Box display="flex" gap={2} mb={3}>
          <Button
            variant={
              selectedView === SelectedView.TotalSales
                ? 'contained'
                : 'outlined'
            }
            onClick={() => handleViewChange(SelectedView.TotalSales)}
          >
            Total Sales
          </Button>
          <Button
            variant={
              selectedView === SelectedView.ByGenre ? 'contained' : 'outlined'
            }
            onClick={() => handleViewChange(SelectedView.ByGenre)}
          >
            By Genre
          </Button>
          <Button
            variant={
              selectedView === SelectedView.ByCountry ? 'contained' : 'outlined'
            }
            onClick={() => handleViewChange(SelectedView.ByCountry)}
          >
            By Country
          </Button>
        </Box>
        {selectedView === SelectedView.TotalSales && <DataByTotalSales />}
        {selectedView === SelectedView.ByGenre && <DataByGenre />}
        {selectedView === SelectedView.ByCountry && <DataByCountry />}
      </LocalizationProvider>
    </Box>
  );
}

export default App;
