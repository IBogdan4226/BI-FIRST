import { CountrySalesData } from '@/types/type';
import { scaleLinear } from 'd3-scale';
import { interpolateReds } from 'd3-scale-chromatic';
import { useState, useRef } from 'react';
import { ComposableMap, Geographies, Geography } from 'react-simple-maps';
import html2pdf from 'html2pdf.js'; // Import the html2pdf.js library
import { Button } from '@mui/material';

const geoUrl = 'https://unpkg.com/world-atlas@2.0.2/countries-110m.json';

export function Map({ countryData }: { countryData: CountrySalesData[] }) {
  const [text, setText] = useState<string>('Click on a country');
  const mapRef = useRef<HTMLDivElement>(null); // Reference to the map container

  const colorScale = scaleLinear()
    .domain([0, Math.max(...countryData.map((item) => item.totalSales))])
    .range([0, 1]);

  const colorInterpolator = interpolateReds;

  const handleExportToPDF = () => {
    if (mapRef.current) {
      // Use html2pdf.js to export the current map view to PDF
      const options = {
        margin: 0.5, // Reduce the margin to fit the map on a single page
        filename: 'country_sales_map.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
          scale: 2, // Adjust the scale for better quality, but not too high to avoid oversized image
          logging: true, // Optional: log the capture process for debugging
          letterRendering: true,
          useCORS: true,
          width: 1000, // Ensure the width is explicitly defined
          height: 500, // Adjust the height to fit the map correctly
        },
        jsPDF: {
          unit: 'in',
          format: 'a4', // Set the page size to A4
          orientation: 'landscape', // Use landscape orientation for a wider map
          compress: true,
        },
      };

      // Export the map content
      html2pdf().from(mapRef.current).set(options).save();
    }
  };

  return (
    <div style={{ textAlign: 'center' }}>
      <h2>{text}</h2>

      {/* Note to inform the user about filters */}
      <p style={{ fontSize: '14px', color: '#888' }}>
        Note: The chart data is updated based on the selected Genre and Date
        filters.
      </p>
      <Button
        variant="contained"
        onClick={handleExportToPDF}
        style={{ marginBottom: '20px' }}
      >
        Export Map to PDF
      </Button>
      <div ref={mapRef} style={{ width: '100%', height: '500px' }}>
        <ComposableMap
          projectionConfig={{
            scale: 155,
          }}
          width={800}
          height={400}
          style={{ width: '100%', height: '100%' }}
        >
          <Geographies geography={geoUrl}>
            {({ geographies }) =>
              geographies.map((geo) => {
                const country = countryData.find(
                  (item) =>
                    mapCountryName(item.customerCountry) === geo.properties.name
                );
                const color = country
                  ? colorInterpolator(colorScale(country.totalSales))
                  : '#ddd';

                return (
                  <Geography
                    key={geo.rsmKey}
                    geography={geo}
                    fill={color}
                    stroke="#FFFFFF"
                    strokeWidth={0.5}
                    onClick={() => {
                      setText(
                        `${geo.properties.name}: ${
                          country ? country.totalSales.toFixed(2) : 'No data'
                        }`
                      );
                    }}
                  />
                );
              })
            }
          </Geographies>
        </ComposableMap>
      </div>
    </div>
  );
}

function mapCountryName(name: string): string {
  const countryNameMapper: { [key: string]: string } = {
    USA: 'United States of America',
    'Czech Republic': 'Czechia',
  };
  return countryNameMapper[name] || name;
}
