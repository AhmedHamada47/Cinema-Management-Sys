open System
open System.IO
open System.Drawing
open System.Windows.Forms
open System.Collections.Generic
open System
open System.IO
open System.Collections.Generic
open System.Windows.Forms
open System.Drawing

// Create the form
let form = new Form(Text = "Cinema Seat Reservation", Size = Size(800, 600))

// Create a panel for the seating area
let panel = new Panel(Dock = DockStyle.Top, BackColor = Color.FromArgb(0, 109, 102), Height = 300)
form.Controls.Add(panel)
form.BackColor <- Color.FromArgb(0, 109, 102)

// Create labels and input controls for booking system
let lblRow = new Label(Text = "Row (A-E):", Location = Point(10, 320))
let lblCol = new Label(Text = "Column (1-8):", Location = Point(10, 350))
let txtRow = new TextBox(Location = Point(120, 320), Width = 300)
let txtCol = new TextBox(Location = Point(120, 350), Width = 300)
let lblCustomer = new Label(Text = "Customer Name:", Location = Point(10, 380))
let txtCustomer = new TextBox(Location = Point(120, 380), Width = 150)
let lblTime = new Label(Text = "Time Slot:", Location = Point(10, 410))
let cmbTimeSlot = new ComboBox(Location = Point(120, 410), Width = 150)
let btnBook = new Button(Text = "Book Seat", Location = Point(50, 450), Width = 100)
let btnLoadReservedSeats = new Button(Text = "Load Reserved Seats", Location = Point(200, 450), Width = 150)

// Add available time slots to ComboBox
let timeSlots = [| "02:00 PM"; "04:00 PM"; "06:00 PM" |]
cmbTimeSlot.Items.AddRange(timeSlots |> Array.map box)

// Add booking controls to the form
form.Controls.AddRange([| lblRow; lblCol; txtRow; txtCol; lblCustomer; txtCustomer; lblTime; cmbTimeSlot; btnBook; btnLoadReservedSeats |])

// Seating chart representation
let rows = 5
let cols = 8
let seatSize = 50
let seatLayout = Array2D.init rows cols (fun row col -> sprintf "%c%d" (char (row + int 'A')) (col + 1))
let reservedSeats = new HashSet<string>()

// Ticket management files based on time slot selection
let getTicketFilePath timeSlot =
    match timeSlot with
    | "02:00 PM" -> "tickets.txt"
    | "04:00 PM" -> "tickets1.txt"
    | "06:00 PM" -> "tickets2.txt"
    | _ -> "tickets.txt" // Default path for any invalid time slot

// Function to load reserved seats from the file
let loadReservedSeats timeSlot =
    let filePath = getTicketFilePath timeSlot
    if File.Exists(filePath) then
        File.ReadLines(filePath)
        |> Seq.iter (fun line ->
            let parts = line.Split(',')
            if parts.Length >= 3 then
                let seat = parts.[1].Split(':').[1].Trim()
                reservedSeats.Add(seat) |> ignore
        )

// Function to save ticket details to a file
let saveTicketDetails ticketID seat customer ticketFilePath =
    let ticketDetails = sprintf "Ticket ID: %s, Seat: %s, Customer: %s" ticketID seat customer
    File.AppendAllText(ticketFilePath, ticketDetails + Environment.NewLine)

// Function to handle seat booking
let bookSeat (row: string) (col: string) (customer: string) =
    try
        let rowIndex = int row.[0] - int 'A'
        let colIndex = int col - 1
        if rowIndex < 0 || rowIndex >= rows || colIndex < 0 || colIndex >= cols then
            MessageBox.Show("Invalid seat coordinates!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        else
            let seat = seatLayout.[rowIndex, colIndex]
            if reservedSeats.Contains(seat) then
                MessageBox.Show(sprintf "Seat %s is already reserved!" seat, "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            else
                reservedSeats.Add(seat) |> ignore
                // Generate a unique ticket ID
                let ticketID = Guid.NewGuid().ToString()
                // Save ticket details
                let ticketFilePath = getTicketFilePath (cmbTimeSlot.SelectedItem :?> string)
                saveTicketDetails ticketID seat customer ticketFilePath
                // Update seat button color
                for button in panel.Controls do
                    match button with
                    | :? Button as btn when btn.Text = seat -> btn.BackColor <- Color.Red
                    | _ -> ()
                MessageBox.Show(sprintf "Seat %s has been reserved.\nTicket ID: %s" seat ticketID, "Reservation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
    with
    | _ -> MessageBox.Show("Invalid input! Please enter a valid row and column.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore

// Event handler for booking button
btnBook.Click.Add(fun _ ->
    let row = txtRow.Text.Trim().ToUpper()
    let col = txtCol.Text.Trim()
    let customer = txtCustomer.Text.Trim()

    // Get the time slot from the ComboBox (cmbTimeSlot)
    let selectedTimeSlot = cmbTimeSlot.SelectedItem :?> string

    // Get the ticket file path based on the selected time slot
    let ticketFilePath = getTicketFilePath selectedTimeSlot

    // Check if required fields are filled
    if row = "" || col = "" || customer = "" then
        MessageBox.Show("All fields are required!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
    else
        bookSeat row col customer
)

// Method to update seat colors
let updateSeatColors () =
    for button in panel.Controls do
        match button with
        | :? Button as btn when reservedSeats.Contains(btn.Text) -> 
            btn.BackColor <- Color.Red  // Reserved seat, color it red
        | :? Button as btn -> 
            btn.BackColor <- Color.FromArgb(191, 255, 251)  // Available seat, default color
        | _ -> ()

// Event handler for Load Reserved Seats button
btnLoadReservedSeats.Click.Add(fun _ ->
    let selectedTimeSlot = cmbTimeSlot.SelectedItem :?> string
    if String.IsNullOrEmpty(selectedTimeSlot) then
        MessageBox.Show("Please select a time slot to load reserved seats.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
    else
        reservedSeats.Clear()  // Clear previous reserved seats
        loadReservedSeats selectedTimeSlot  // Load new reserved seats for the selected time slot
        
        // Update seat colors for reserved seats after loading
        updateSeatColors()
        
        MessageBox.Show("Reserved seats have been loaded.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
)

// Create seats dynamically
for row in 0 .. rows - 1 do
    for col in 0 .. cols - 1 do
        let seatName = seatLayout.[row, col]
        let button = new Button(Text = seatName, Size = Size(seatSize, seatSize), Location = Point(col * seatSize + 10, row * seatSize + 10))
        button.BackColor <- Color.FromArgb(191, 255, 251)
        panel.Controls.Add(button)

// Run the application
Application.Run(form)
