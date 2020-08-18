using MvvmCross;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bluetooth_DEMO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _adapter;
        IBluetoothLE bluetoothBLE;
        ObservableCollection<IDevice> list;
        IDevice device;

        public MainPage()
        {
            InitializeComponent();

            bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            list = new ObservableCollection<IDevice>();
            DevicesList.ItemsSource = list;
        }

        private async void searchDevice(object sender, EventArgs e)
        {
            if (bluetoothBLE.State == BluetoothState.Off)
            {
                await DisplayAlert("Espera..", "Bluetooth deshabilitado.", "OK");
            }
            else
            {

                try
                {
                    await _adapter.StartScanningForDevicesAsync();
                    list.Clear();
                    _adapter.ScanTimeout = 10000;
                    _adapter.ScanMode = ScanMode.Passive;
                    _adapter.DeviceDiscovered += (s, a) =>
                    {
                        /* if (!list.Contains(a.Device))
                         {*/
                        list.Add(a.Device);
                        // }

                    };


                    if (!bluetoothBLE.Adapter.IsScanning)
                    {
                        await _adapter.StartScanningForDevicesAsync();


                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Notice", ex.Message.ToString(), "Error !");
                }
            }

        }


        private async void DevicesList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            IDevice selecteditem = DevicesList.SelectedItem as IDevice;

            try
            {
                await _adapter.ConnectToDeviceAsync(selecteditem);
                await DisplayAlert("Conectado", "Status:" + device.State, "OK");
            }
            catch (DeviceConnectionException ex)
            {

            }


        }
        IReadOnlyList<IService> Services;
        IService Service;
        private async void getServices(object sender, EventArgs e)
        {
            Services = await device.GetServicesAsync();
            Service = await device.GetServiceAsync(device.Id);

        }

        IList<ICharacteristic> Characteristics;
        ICharacteristic Characteristic;
        private async void btnGetcharacters_Clicked(object sender, EventArgs e)
        {
            var characteristics = await Service.GetCharacteristicsAsync();
            Guid idGuid = Guid.Parse("guid");
            Characteristic = await Service.GetCharacteristicAsync(idGuid);
        }

        IDescriptor descriptor;
        IList<IDescriptor> descriptors;

        private async void btnDescriptors_Clicked(object sender, EventArgs e)
        {
            descriptors = (IList<IDescriptor>)await Characteristic.GetDescriptorsAsync();
            descriptor = await Characteristic.GetDescriptorAsync(Guid.Parse("guid"));

        }

        private async void btnDescRW_Clicked(object sender, EventArgs e)
        {
            var bytes = await descriptor.ReadAsync();
            await descriptor.WriteAsync(bytes);
        }

        private async void btnGetRW_Clicked(object sender, EventArgs e)
        {
            var bytes = await Characteristic.ReadAsync();
            await Characteristic.WriteAsync(bytes);
        }

        private async void btnGetDevices_Clicked(object sender, EventArgs e)
        {
            IReadOnlyList<IDevice> systemDevices = _adapter.GetSystemConnectedOrPairedDevices();
            foreach (var device in systemDevices)
            {
                await _adapter.ConnectToDeviceAsync(device);
            }
        }

        private async void btnKnowConnect_Clicked(object sender, EventArgs e)
        {

            try
            {
                await _adapter.ConnectToKnownDeviceAsync(new Guid("guid"));

            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Notice", ex.Message.ToString(), "OK");
            }
        }
    }
}
