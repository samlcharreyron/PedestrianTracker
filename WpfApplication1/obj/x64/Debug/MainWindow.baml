   M S B A M L   `   `   `  ����L  HPedestrianTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?)clr-namespace:PedestrianTracker;assembly=PedestrianTracker  5   	   "    PedestrianTracker.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   7l)clr-namespace:PedestrianTracker;assembly=     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � Title$  Pedestrian Counter��5   	    � Icon$ icons/pedtrac.ico��5	   	   $	��670��5
   	   $	��650��5   	   $	��670��5   	    � 
ResizeMode$ CanResizeWithGrip=�5   	    � 	Resources 5      +   %PedestrianTracker.VisibilityConverter   VisibilityConverter2  ,PedestrianTracker.RoadAxisY1PaddingConverter  RoadAxisY1PaddingConverter2  ,PedestrianTracker.RoadAxisY2PaddingConverter  RoadAxisY2PaddingConverter��%W   &	        &	      &	 &       5   
   6      5   
   6'     5   
   6'   5      .��5      � -   5        $��MainGrid��5       � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   ]� .+�5   
   o� -   5        $��menu1��5      $��0��6   $��22��6)   .@�5      m� $��_File��6   .@�5      m� -   5        $��_Open��5       m� Icon 5      �� 5       $��icons\openHS.png��6$   $��16��6>   $��16��6J   5"      m� 5$      -   5        $��_Save As��5$       5%      �� 5&      $��icons\saveHS.png��6$   $��16��6>   $��16��6J   5(      m� 5)      -   5        $��
Save Video��5)      63   �� 5*      6"   m� 5+      -   5        $��E_xit��5+      6*   5,      m� 5-      $��_View��6   .@�5.      m� -   5        $��Show Tracked Data��5.      6:   m� 5/      -	   5        $��Show Past Trajectories��5/       m� IsCheckable . 6�   m� 50      -
   5        $��Show Road Axis��50       . 6p   m� 51      -   5        $��Show 3D Trajectory View��51      6@   52      m� 53      $��_Tools��6   .@�54      m� -   5        $��Options��54      60   56      57      58      9� 5=   
   -   5         9�Name$ Expander��5=       9� ExpandDirection$
 Down=�6%   $��1��6<   .��5>      �� -   5        	 ��Name$	 C1��5>      .+�5?      �� -   5        $��image1��5?      ��6&   �� 
 �� Path$
 RenderHeight���6L   �� $
 RenderHeight�'� 5@      -   5        $��viewport��5@      ��6-   ��  �� ElementName$ image1��$
 ActualHeight���6f   �� $ image1��$
 ActualWidth� '� Camera 5A      =� 5B      -   5        
camera�� � LookDirection$ 0,0,-15�5C       � UpDirection$ 0,1,05�64    � Position$ 0,0,01�5D       =� FieldOfView$ 45Q�6.   .x�5F      g� -   5        	model�� g� Content 5G      h� 5H      -   5        	group��.��5I   "   ��  �� Color$ DarkGray��6/   t� 5J   "   $ White��63    t� 	Direction$ -5,-5,-75�6A   5K      5M      5N      "   PedestrianTracker.Trajectory  5O      -   5          Name$ trajectoryCanvas1��5O      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight�  5P      -   5        $ trajectoryCanvas2��5P      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight�  5Q      -   5        $ trajectoryCanvas3��5Q      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight�  5R      -   5        $ trajectoryCanvas4��5R      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight�  5S      -   5        $ trajectoryCanvas5��5S      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight�  5T      -   5        $ trajectoryCanvas6��5T      ��6:   �� $ image1��$
 ActualWidth���6p   �� $ image1��$
 ActualHeight��� 5U      -   5         ��Name$ pastTrajectories��5U      ��62   �� $ image1��$
 ActualWidth���6i   �� $ image1��$
 ActualHeight�5V      5W      � 5Y   
   -   5        $��MessageGrid��5Y      $��3��6"   $��30��6/   $��Bottom=�6;    � ColumnDefinitions 5Z      �� 5[      $��* �6#   �� 5\      $��* �6#   �� 5]      $��* �6#   .+�5_      ��  �� Orientation$ 
Horizontal=�6   $��0��62   .+�5`      �� -   5        $��TotalPlayersLabel��5`      $��People in Frame: ��65   $��20(�6N   �� 5a      -   5        $��TotalPlayersDisplay��5a      $��20(�6Z   ��67   �� $
 TotalPlayers�6Z   5b      �� 5c      $ 
Horizontal=�6   $��1��62   .+�5d      �� $��   Pedestrian Counts: ��6   $��20(�6;   �� 5e      -   5        $��PedestrianCountsDisplay��5e      $��20(�6b   ��6;   �� $
 PedestrianCounts�6b   5f      �� 5k      $ 
Horizontal=�6   $��2��62   .+�5l      �� $��   Frame Rate: ��6   $��20(�64   �� 5m      -   5        $��FrameRateDisplay��5m      $��20(�6T   ��64   �� $
 	FrameRate�6T   5n      �� 5o      -    5        $��errorBox��5o      $��1��6(   $��20(�68   
���  ��6F   5p      5q      5r      